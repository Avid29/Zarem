// Avishai Dernis 2025

using CommunityToolkit.Diagnostics;
using CommunityToolkit.HighPerformance;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Zarem.Assembler.Extensions;
using Zarem.Assembler.Extensions.System;
using Zarem.Assembler.Helpers.Tables;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Assembler.Models;
using Zarem.Assembler.Models.Abstract;
using Zarem.Assembler.Models.Meta;
using Zarem.Assembler.Parsers;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Assembler.Tokenization.Models.Enums;
using Zarem.Extensions;
using Zarem.Helpers;
using Zarem.Models;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Instructions.Enums.SpecialFunctions;
using Zarem.Models.Tables;
using Zarem.Models.Tables.Enums;

namespace Zarem.Assembler;

/// <summary>
/// A struct for parsing MIPS instructions.
/// </summary>
public struct MipsInstructionParser
{
    private readonly MipsAssemblerConfig? _config;
    private readonly Address _currentAddress;
    private readonly IReadOnlyDictionary<string, Symbol>? _symbols;
    private readonly MipsInstructionTable _instructionTable;
    private readonly AssemblerLogger? _logger;

    private MipsInstructionMetaBase? _meta;

    private GPRegister _rs;
    private GPRegister _rt;
    private GPRegister _rd;
    private FloatFormat _format;
    private int _immediate;
    private List<RelocationEntry>? _references;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsInstructionParser"/> struct.
    /// </summary>
    public MipsInstructionParser(MipsAssemblerConfig config, MipsInstructionTable? table, Address address, IReadOnlyDictionary<string, Symbol>? symbols,  ILogger? logger)
    {
        _config = config;
        _currentAddress = address;
        _symbols = symbols;
        _references = [];

        _instructionTable = table ?? new MipsInstructionTable(config);

        if (logger is not null)
        {
            _logger = new AssemblerLogger(logger);
        }
    }

    /// <summary>
    /// Attempts to parse an instruction from a name and a list of arguments.
    /// </summary>
    /// <param name="line">The assembly line to parse.</param>
    /// <returns>The parser instruction.</returns>
    public MipsParsedInstruction? Parse(AssemblyLine line)
    {
        // Attempt to load the instruction
        // If successful, this will set the _meta and _format
        if (!TryParseInstruction(line, out var name))
            return null;

        // Applies provided values
        _rs = (GPRegister)(_meta.FixedRS ?? default);
        _rt = (GPRegister)(_meta.FixedRT ?? default);
        _rd = (GPRegister)(_meta.FixedRD ?? default);

        // Parse argument data according to pattern
        Argument[] pattern = _meta.ArgumentPattern;
        for (int i = 0; i < line.Args.Count; i++)
        {
            // Split out next arg
            var arg = line.Args[i];

            // Empty argument
            if (arg.Tokens.Length is 0)
            {
                var reportToken = arg.ProceedingComma ?? arg.PrecedingComma;
                Guard.IsNotNull(reportToken);
                _logger?.Log(Severity.Error, LogId.InvalidInstructionArg, reportToken, "EmptyArgument");
                continue;
            }

            TryParseArg(arg.Tokens, pattern[i]);
        }

        // It's a pseudo instruction.
        // Create a pseudo-instruction and return with reference
        // as parsed instruction.
        if (_meta is PseudoInstructionMeta pMeta)
        {
            var pseudo = new PseudoInstruction
            {
                PseudoOp = pMeta.PseudoOp,
                RS = _rs,
                RT = _rt,
                RD = _rd,
                Immediate = _immediate,
                Address = (uint)_immediate,
            };

            return new MipsParsedInstruction(pseudo, _references);
        }

        // Build an instruction using the information from
        // _meta and all the parsed arguments
        var instruction = BuildInstruction();

        // Check for write back to zero register
        // Give a warning if not an explicit nop operation
        // TODO: Check on pseudo-instructions
        if (instruction.GetWritebackRegister() is GPRegister.Zero && name != "nop")
        {
            // Only log if the token can be parsed, and is not 0 for other reasons
            // TODO: Is this true for move operations? Double check
            var writebackArg = line.Args[0].Tokens;
            if (writebackArg.Length is 1 && TryParseRegister(writebackArg[0], out var reg) && reg is GPRegister.Zero)
            {
                _logger?.Log(Severity.Message, LogId.ZeroRegWriteback, writebackArg, "ZeroRegisterWriteback");
            }

        }

        return new MipsParsedInstruction(instruction, _references);
    }

    [MemberNotNullWhen(true, nameof(_meta))]
    private bool TryParseInstruction(AssemblyLine line, [NotNullWhen(true)] out string? name)
    {
        // Get instruction name and ensure it's not null
        name = line.Instruction?.Source;
        Guard.IsNotNull(line.Instruction);
        Guard.IsNotNull(name);

        // Parse out format from instruction name if present
        if (FloatFormatTable.TryGetFloatFormat(name, out _format, out var formattedName))
            name = formattedName;

        if (!_instructionTable.TryGetInstruction(name, out var metas, out var version, out var is64bit, out var banned))
        {
            (LogId id, string message) = version switch
            {
                not null when banned => (LogId.DisabledFeatureInUse, "InstructionDisabled"),
                not null when _config is null || version > _config.Version => (LogId.NotInVersion, "RequiresVersion"),
                not null => (LogId.NotInVersion, "RemovedInVersion"),
                null when _config is not null && is64bit && !_config.Version.Is64Bit() => (LogId.NotInVersion, "Needs64BitVersion"),
                null => (LogId.InvalidInstructionName, "NoInstructionNamed")
            };

            _logger?.Log(Severity.Error, id, line.Instruction, message, name, $"{version:d}");
            return false;
        }

        _meta = metas.FirstOrDefault(x => x.ArgumentPattern.Length == line.Args.Count);

        if (_meta is null)
        {
            _logger?.Log(Severity.Error, LogId.InvalidInstructionArgCount, line.Instruction, "WrongArgumentCount", name, line.Args.Count);
            return false;
        }

        // Check float format support via the specialized Float record
        if (_meta is FloatInstructionMeta fMeta && fMeta.SupportedFormats is not null && !fMeta.SupportedFormats.Contains(_format))
        {
            _logger?.Log(Severity.Error, LogId.InvalidFloatFormat, line.Instruction, $"DoesNotSupportFormat{_format}", name);
            return false;
        }

        return true;
    }

    private bool TryParseArg(ReadOnlySpan<Token> arg, Argument type)
    {
        return type switch
        {
            // Register arguments
            (>= Argument.RS and <= Argument.RD) or
            (>= Argument.FS and <= Argument.FD) or
            Argument.RT_Numbered => TryParseRegisterArg(arg, type),

            // Expression arguments
            Argument.ShiftAmount or Argument.Immediate or Argument.FullImmediate
            or Argument.Offset or Argument.LargeOffset or Argument.Address => TryParseExpressionArg(arg, type),

            // Address offset arguments
            Argument.AddressBase => TryParseAddressOffsetArg(arg),

            _ => ThrowHelper.ThrowArgumentOutOfRangeException<bool>($"Argument of type '{type}' is not within parsable type range."),
        };
    }

    /// <summary>
    /// Parses an argument as a register and assigns it to the target component.
    /// </summary>
    private bool TryParseRegisterArg(ReadOnlySpan<Token> arg, Argument target)
    {
        if (arg.Length is not 1)
        {
            _logger?.Log(Severity.Error, LogId.InvalidRegisterArgument, arg, "ArgumentNotARegister", arg.Print());
            return false;
        }

        // Get reference to selected register argument
        RefTuple<Ref<GPRegister>, RegisterSet> pair = target switch
        {
            // General Purpose Registers
            Argument.RS => new(new(ref _rs), RegisterSet.GeneralPurpose),
            Argument.RT => new(new(ref _rt), RegisterSet.GeneralPurpose),
            Argument.RD => new(new(ref _rd), RegisterSet.GeneralPurpose),
            // Float Registers
            Argument.FS => new(new(ref _rs), RegisterSet.FloatingPoints),
            Argument.FT => new(new(ref _rt), RegisterSet.FloatingPoints),
            Argument.FD => new(new(ref _rd), RegisterSet.FloatingPoints),
            // RT Register for coprocessors
            Argument.RT_Numbered => new(new(ref _rt), RegisterSet.Numbered),
            // Invalid target type
            _ => throw new ArgumentOutOfRangeException($"Argument of type '{target}' attempted to parse as a register.")
        };

        (Ref<GPRegister> regRef, RegisterSet set) = pair;
        ref GPRegister reg = ref regRef.Value;

        if (!TryParseRegister(arg[0], out var register, set))
        {
            // Register could not be parsed.
            // Error already logged.

            return false;
        }

        // Cache register as appropriate argument type
        reg = register;

        return true;
    }

    /// <summary>
    /// Parses an argument as an expression and assigns it to the target component
    /// </summary>
    private bool TryParseExpressionArg(ReadOnlySpan<Token> arg, Argument target)
    {
        // Attempt to parse expression
        if (!ExpressionParser.TryParse<long>(arg, out var expResult, _symbols, _logger?.Parent))
            return false;

        if (expResult.IsSymbolic && target is Argument.ShiftAmount)
        {
            // TODO: Consider tracking ref symbol token
            _logger?.Log(Severity.Error, LogId.InvalidRelocatable, arg, "RelocatableShiftAmount");
            return false;
        }

        if (expResult.IsSymbolic)
        {
            // NOTE: If it were possible to have an undeclared local system reference,
            // it would be here. However, since there's not forward declaration of local
            // symbols, that's not a consider

            var type = target switch
            {
                Argument.Address => MipsReferenceType.JumpTarget26,
                Argument.Immediate => MipsReferenceType.Low16,
                Argument.Offset => MipsReferenceType.PCRelative16,
                Argument.LargeOffset => MipsReferenceType.PCRelative26,
                // FullImmediate triggers a HI/LO pair
                Argument.FullImmediate => MipsReferenceType.High16,
                _ => ThrowHelper.ThrowArgumentOutOfRangeException<MipsReferenceType>($"Argument of type '{target}' cannot reference relocatable symbols."),
            };

            _references ??= [];
            var symbol = expResult.Symbol;
            var addend = (uint)expResult.Addend;

            if (target is Argument.FullImmediate)
            {
                _references.Add(new RelocationEntry(symbol.Name, _currentAddress, (uint)MipsReferenceType.High16, addend));
                _references.Add(new RelocationEntry(symbol.Name, _currentAddress + 4, (uint)MipsReferenceType.Low16, addend));
            }
            else
            {
                // Standard single relocation
                _references.Add(new RelocationEntry(symbol.Name, _currentAddress, (uint)type, addend));
            }
        }

        // NOTE: Casting might truncate the value to fit the bit size.
        // This is the desired behavior, but when logging errors this
        // should be handled explicitly and drop an assembler warning.
        //
        // ALSO NOTE: The linker will fill in the added if the value
        // is symbolic (not absolute)

        long value = expResult.IsAbsolute ? expResult.Addend : 0;

        // Truncates the value to fit the target argument
        CleanInteger(ref value, arg, target);

        // Assign to appropriate instruction argument
        switch (target)
        {
            case Argument.ShiftAmount:
                _immediate = (byte)value;
                return true;
            case Argument.Immediate:
                _immediate = (short)value;
                return true;
            case Argument.FullImmediate:
                _immediate = (int)value;
                return true;
            case Argument.Address:
                _immediate = (int)(uint)value;
                return true;
            case Argument.Offset:
            case Argument.LargeOffset:
                _immediate = (int)value;
                return true;

            // Invalid target type
            default:
                return ThrowHelper.ThrowArgumentOutOfRangeException<bool>($"Argument '{arg.Print()}' of type '{target}' attempted to parse as an expression.");
        }
    }

    /// <summary>
    /// Parses an argument as an address offset, assigning its components to immediate and $rs.
    /// </summary>
    private bool TryParseAddressOffsetArg(ReadOnlySpan<Token> arg)
    {
        // NOTE: Be careful about forwards to other parse functions with regards to 
        // error logging. Address offset argument errors might be inappropriately logged.

        // Split the string into an offset and a register, return false if failed
        if (!SplitAddressOffset(arg, out var offsetStr, out var regStr))
            return false;

        // Try parse offset component into immediate, return false if failed
        if (!TryParseExpressionArg(offsetStr, Argument.Immediate))
            return false;

        // Parse register component into $rs, return false if failed
        if (!TryParseRegisterArg(regStr, Argument.RS))
            return false;

        return true;
    }

    private readonly bool TryParseRegister(Token arg, out GPRegister register, RegisterSet set = RegisterSet.GeneralPurpose)
    {
        register = GPRegister.Zero;

        // Check that argument is register argument
        if (arg.Type is not TokenType.Register)
        {
            _logger?.Log(Severity.Error, LogId.InvalidRegisterArgument, arg, "ArgumentNotARegister", arg);
            return false;
        }

        // Get named register from table
        if (!MipsRegisterTable.Instance.TryGetRegister(arg.Source, out register, out RegisterSet parsedSet))
        {
            // Register does not exist in table
            _logger?.Log(Severity.Error, LogId.InvalidRegisterArgument, arg, "RegisterNotFound", arg);
            return false;
        }

        if (register is >= (GPRegister)32)
        {
            var (message, msgArg) = parsedSet switch
            {
                RegisterSet.Numbered => ("RegisterNumberNotFound", (object)(int)register),
                _ => ("RegisterNotIndexable", arg)
            };

            _logger?.Log(Severity.Error, LogId.InvalidRegisterArgument, arg, message, msgArg);
            return false;
        }

        // Match register set
        if (parsedSet != RegisterSet.Numbered && parsedSet != set)
        {
            _logger?.Log(Severity.Error, LogId.InvalidRegisterArgument, arg, $"RegisterMustBeIn{set}Set", arg);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Splits an address offset argument into a token span for the offset and the address register token.
    /// </summary>
    /// <remarks>
    /// Upon return offset and register do not need to be valid offset and register strings.
    /// The register is just the component in parenthesis. The offset is just the component before the parenthesis.
    /// Nothing may follow the parenthesis.
    /// </remarks>
    private readonly bool SplitAddressOffset(ReadOnlySpan<Token> arg, out ReadOnlySpan<Token> offset, out ReadOnlySpan<Token> register)
    {
        offset = arg;
        register = [];

        // Find matched parenthesis start and end
        var parIndex = arg.FindNext(TokenType.OpenParenthesis, out _);
        var closeIndex = arg.FindNext(TokenType.CloseParenthesis, out _);
        if (parIndex is -1 || closeIndex is -1)
        {
            // TODO: Improve messaging
            _logger?.Log(Severity.Error, LogId.InvalidAddressOffsetArgument, arg, "InvalidAddressOffsetArgument", arg.Print());
            return false;
        }

        // Offset is everything before the parenthesis
        offset = arg[..parIndex];

        // Register is everything between the parenthesis
        register = arg[(parIndex + 1)..closeIndex];

        // Ensure there's no content following the parenthesis.
        if (!arg[(closeIndex + 1)..].IsEmpty)
        {
            // TODO: Improve messaging
            _logger?.Log(Severity.Error, LogId.InvalidAddressOffsetArgument, arg, "InvalidAddressOffsetArgument", arg.Print());
            return false;
        }

        return true;
    }

    private readonly void CleanInteger(ref long value, ReadOnlySpan<Token> arg, Argument target)
    {
        // Determine casting details for the argument
        (int bitCount, int shiftAmount, bool signed) = target switch
        {
            Argument.ShiftAmount => (5, 0, false),
            Argument.Offset => (16, 2, false),
            Argument.Immediate => (16, 0, true),
            Argument.Address => (26, 2, false),
            Argument.LargeOffset => (26, 2, true),
            Argument.FullImmediate => (32, 0, true),
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<(byte, byte, bool)>($"Argument of type '{target}' attempted to parse as an expression."),
        };

        // Clean integer to fit within argument bit size and match signs
        // Log a message if the value was truncated and/or had its sign changed
        long original = value;
        if (!long.TryCast(ref value, bitCount, shiftAmount, signed, out var changes))
        {
            _logger?.Log(Severity.Warning, LogId.IntegerTruncated, arg, $"CastWarning{changes}", arg.Print(), original, value, bitCount, shiftAmount);
        }
    }

    private readonly MipsInstruction BuildInstruction()
    {
        Guard.IsNotNull(_meta);

        return _meta switch
        {
            RTypeInstructionMeta spec => MipsInstruction.Create((byte)spec.OperationCode, (byte)spec.FuncCode, _rs, _rt, _rd, (byte)_immediate),

            RegImmInstructionMeta ri =>
                (ri.RtCode is >= RegImmFuncCode.BranchOnLessThanZero and <= RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikely) ||
                (ri.RtCode is >= RegImmFuncCode.BranchOnLessThanZeroAndLink and <= RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikelyAndLink)
                ? MipsInstruction.Create(ri.RtCode, _rs, _immediate)
                : MipsInstruction.Create(ri.RtCode, _rs, (short)_immediate),

            CoProc0InstructionsMeta c0 when c0.Mfmc0FuncCode.HasValue => CoProc0Instruction.Create(c0.Mfmc0FuncCode.Value, _rt, (byte)_rd),
            CoProc0InstructionsMeta c0 when c0.FuncCode.HasValue => CoProc0Instruction.Create(c0.FuncCode.Value, _rd),
            CoProc0InstructionsMeta c0 => CoProc0Instruction.Create(c0.RSCode, _rt, _rd),

            CoProc1InstructionsMeta c1 => FloatInstruction.Create(c1.RSCode, _rt, (FloatRegister)_rs),
            FloatInstructionMeta f => FloatInstruction.Create(f.Function, _format, (FloatRegister)_rs, (FloatRegister)_rd, (FloatRegister)_rt),

            ITypeInstructionMeta std => std.OperationCode switch
            {
                OperationCode.Jump or OperationCode.JumpAndLink or OperationCode.JumpAndLinkX
                    => MipsInstruction.Create(std.OperationCode, (uint)_immediate),

                OperationCode.BranchCompact or OperationCode.BranchAndLinkCompact
                    => throw new NotImplementedException(),

                var op when 
                    op is (>= OperationCode.BranchOnEquals and <= OperationCode.BranchOnGreaterThanZero) or
                          (>= OperationCode.BranchOnEqualLikely and <= OperationCode.BranchOnGreaterThanZeroLikely)
                    => MipsInstruction.Create(op, _rs, _rt, _immediate),

                _ => MipsInstruction.Create(std.OperationCode, _rs, _rt, (short)_immediate)
            },

            _ => throw new NotSupportedException($"Metadata type {_meta.GetType().Name} is not supported for encoding.")
        };
    }
}
