// Avishai Dernis 2025

using CommunityToolkit.Diagnostics;
using CommunityToolkit.HighPerformance;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Zarem.Assembler.Config;
using Zarem.Assembler.Extensions.System;
using Zarem.Assembler.Helpers.Tables;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Assembler.Models;
using Zarem.Assembler.Parsers.Enums;
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

namespace Zarem.Assembler.Parsers;

/// <summary>
/// A struct for parsing instructions.
/// </summary>
public struct MipsInstructionParser
{
    private readonly MipsAssemblerConfig? _config;
    private readonly Address _currentAddress;
    private readonly IReadOnlyDictionary<string, Symbol>? _symbols;
    private readonly InstructionTable _instructionTable;
    private readonly AssemblerLogger? _logger;

    private MipsInstructionMetadata _meta;

    private GPRegister _rs;
    private GPRegister _rt;
    private GPRegister _rd;
    private FloatFormat _format;
    private byte _shift;
    private int _immediate;
    private uint _address;
    private List<RelocationEntry>? _references;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsInstructionParser"/> struct.
    /// </summary>
    public MipsInstructionParser(MipsAssemblerConfig config, InstructionTable? table, Address address, IReadOnlyDictionary<string, Symbol>? symbols,  ILogger? logger)
    {
        _config = config;
        _currentAddress = address;
        _symbols = symbols;
        _references = [];

        _instructionTable = table ?? new InstructionTable(config);

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
        _rs = (GPRegister)(_meta.RS ?? default);
        _rt = (GPRegister)(_meta.RT ?? default);
        _rd = (GPRegister)(_meta.RD ?? default);

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
        if (_meta.IsPseudoInstruction)
        {
            Guard.IsTrue(_meta.PseudoOp.HasValue);

            var pseudo = new PseudoInstruction
            {
                PseudoOp = _meta.PseudoOp.Value,
                RS = _rs,
                RT = _rt,
                RD = _rd,
                Immediate = _immediate,
                Address = _address,
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

    private bool TryParseInstruction(AssemblyLine line, [NotNullWhen(true)] out string? name)
    {
        // Get instruction name and ensure it's not null
        name = line.Instruction?.Source;
        Guard.IsNotNull(line.Instruction);
        Guard.IsNotNull(name);

        // Parse out format from instruction name if present
        if (FloatFormatTable.TryGetFloatFormat(name, out _format, out var formattedName))
            name = formattedName;

        if (!_instructionTable.TryGetInstruction(name, out var metas, out var version, out var banned))
        {
            // Select error message
            (LogId id, string message) = version switch
            {
                not null when banned => 
                    (LogId.DisabledFeatureInUse, "InstructionDisabled"),

                // The instruction requires a higher MIPS version
                not null when _config is null || version > _config.MipsVersion =>
                    (LogId.NotInVersion, "RequiresVersion"),

                // The instruction is deprecated
                not null => (LogId.NotInVersion, "RemovedInVersion"),

                // The instruction does not exist.
                null => (LogId.InvalidInstructionName, "NoInstructionNamed")
            };

            // Log the error
            // TODO: Improve version formatting
            _logger?.Log(Severity.Error, id, line.Instruction, message, name, $"{version:d}");
            return false;
        }

        // Assert instruction metadata with proper argument count exists
        if (!metas.Any(x => x.ArgumentPattern.Length == line.Args.Count))
        {
            // TODO: Improve messaging
            //var message = line.Args.Count < pattern.Length
            //    ? $"Instruction '{name}' doesn't have enough arguments. Found {line.Args.Count} arguments when expecting {_meta.ArgumentPattern.Length}."
            //    : $"Instruction '{name}' has too many arguments! Found {line.Args.Count} arguments when expecting {_meta.ArgumentPattern.Length}.";

            _logger?.Log(Severity.Error, LogId.InvalidInstructionArgCount, line.Instruction, "WrongArgumentCount", name, line.Args.Count);
            return false;
        }

        // Find instruction pattern with matching argument count
        _meta = metas.FirstOrDefault(x => x.ArgumentPattern.Length == line.Args.Count);

        // Check that the float format is supported valid with the instruction, if applicable
        if (_meta.FloatFormats is not null && !_meta.FloatFormats.Contains(_format))
        {
            // TODO: Should float format be a separate token?
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
            Argument.Shift or Argument.Immediate or Argument.FullImmediate
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
        if (!ExpressionParser.TryParse(arg, out var expResult, _symbols, _logger?.Parent))
            return false;

        if (expResult.IsSymbolic && target is Argument.Shift)
        {
            // TODO: Consider tracking ref symbol token
            _logger?.Log(Severity.Error, LogId.RelocatableReferenceInShift, arg, "RelocatableShiftAmount");
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
            case Argument.Shift:
                _shift = (byte)value;
                return true;
            case Argument.Immediate:
                _immediate = (short)value;
                return true;
            case Argument.FullImmediate:
                _immediate = (int)value;
                return true;
            case Argument.Address:
                _address = (uint)value;
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
        var regStr = arg.Source;
        if (regStr[0] != '$')
        {
            _logger?.Log(Severity.Error, LogId.InvalidRegisterArgument, arg, "ArgumentNotARegister", arg);
            return false;
        }

        // Get named register from table
        if (!RegistersTable.TryGetRegister(regStr, out register, out RegisterSet parsedSet))
        {
            // Register does not exist in table
            _logger?.Log(Severity.Error, LogId.InvalidRegisterArgument, arg, "RegisterNotFound", arg);
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
            Argument.Shift => (5, 0, false),
            Argument.Offset => (16, 2, false),
            Argument.Immediate => (16, 0, true),
            Argument.Address => (26, 2, false),
            Argument.LargeOffset => (26, 2, true),
            Argument.FullImmediate => (32, 0, true),
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<(byte, byte, bool)>($"Argument of type '{target}' attempted to parse as an expression."),
        };

        // Clean integer to fit within argument bit size and match signs
        long original = value;
        var cleanStatus = CastInteger(ref value, bitCount, shiftAmount, signed);

        // Log a message if the value was truncated and/or had its sign changed
        if (cleanStatus is not CastingChanges.None)
        {
            _logger?.Log(Severity.Warning, LogId.IntegerTruncated, arg, $"CastWarning{cleanStatus}", arg.Print(), original, value, bitCount, shiftAmount);
        }
    }

    /// <remarks>
    /// This does not apply the <paramref name="shiftAmount"/>! It only masks the lower bits.
    /// </remarks>
    /// <param name="integer">A reference to the integer to modify.</param>
    /// <param name="bitCount">The number of bits after casting.</param>
    /// <param name="shiftAmount">The number of bits that will drop from the bottom.</param>
    /// <param name="signed">Whether or not the new value should be signed.</param>
    /// <returns>The changes made to the integer.</returns>
    private static CastingChanges CastInteger(ref long integer, int bitCount, int shiftAmount, bool signed = false)
    {
        var original = integer;

        Guard.IsGreaterThan(bitCount, 1);
        Guard.IsLessThanOrEqualTo(bitCount + shiftAmount, 64);

        // Create a masks for the high and low truncating bits,
        // as well as an overall remaining bits map
        var upperMask = bitCount == 64 ? -1L : (1L << (bitCount + shiftAmount)) - 1;
        var lowerMask = ~((1L << shiftAmount) - 1);
        var mask = (upperMask & lowerMask);

        // Truncate mask upper and lower bits
        long truncated = integer & mask;

        // Sign extend if signed and not full width
        if (signed && bitCount < 64)
        {
            long signBit = 1L << (bitCount - 1);
            if ((truncated & signBit) != 0)
                truncated |= ~upperMask; // Sign extend
        }

        integer = truncated;

        // Compute changes
        var changes = CastingChanges.None;

        // Check if the sign was dropped
        if (!signed && original < 0)
            changes |= CastingChanges.SignChanged;

        // Check for upper truncation
        long upperBits = original & ~upperMask;
        if (upperBits != 0 && upperBits != ~upperMask)
            changes |= CastingChanges.TruncatedHigh;

        // Check for lower truncation
        if ((original & ~lowerMask) != 0)
        {
            changes |= CastingChanges.TruncatedLow;
        }

        // Return combined code
        return changes;
    }

    private readonly MipsInstruction BuildInstruction()
    {
        // If it's not a pseudo instruction, there should be an OpCode
        Guard.IsNotNull(_meta.OpCode);

        // Create the instruction from its components based on the instruction type
        return _meta.OpCode switch
        {
            // R Type
            OperationCode.Special => _meta.FuncCode.HasValue ?                              // Special
                MipsInstruction.Create(_meta.FuncCode.Value, _rs, _rt, _rd, _shift) :
                _ = ThrowHelper.ThrowArgumentException<MipsInstruction>($"Instructions with OpCode:{_meta.OpCode} must have a {nameof(_meta.FuncCode)} value."),
            OperationCode.Special2 => _meta.Function2Code.HasValue ?                        // Special 2
                MipsInstruction.Create(_meta.Function2Code.Value, _rs, _rt, _rd, _shift) :
                _ = ThrowHelper.ThrowArgumentException<MipsInstruction>($"Instructions with OpCode:{_meta.OpCode} must have a {nameof(_meta.Function2Code)} value."),
            OperationCode.Special3 => _meta.Function3Code.HasValue ?                        // Special 3
                MipsInstruction.Create(_meta.Function3Code.Value, _rs, _rt, _rd, _shift) :
                _ = ThrowHelper.ThrowArgumentException<MipsInstruction>($"Instructions with OpCode:{_meta.OpCode} must have a {nameof(_meta.Function3Code)} value."),

            // J Type
            OperationCode.Jump or OperationCode.JumpAndLink
            or OperationCode.JumpAndLinkX => MipsInstruction.Create(_meta.OpCode.Value, _address),

            // Coprocessor0 instructions
            OperationCode.Coprocessor0 when _meta.Co0FuncCode.HasValue                      // C0
                => CoProc0Instruction.Create(_meta.Co0FuncCode.Value, _rd),
            OperationCode.Coprocessor0 when _meta.Mfmc0FuncCode.HasValue                    // MFMC0
                => CoProc0Instruction.Create(_meta.Mfmc0FuncCode.Value, _rt, _meta.RD),
            OperationCode.Coprocessor0 => _meta.CoProc0RS.HasValue ?                        // Co0 RS
                CoProc0Instruction.Create(_meta.CoProc0RS.Value, _rt, _rd) :
                _ = ThrowHelper.ThrowArgumentException<MipsInstruction>($"Instructions with OpCode:{_meta.OpCode} must have a {nameof(_meta.CoProc0RS)}, {nameof(_meta.Co0FuncCode)}, or {nameof(_meta.Mfmc0FuncCode)} value."),

            // FloatingPoint instructions
            OperationCode.Coprocessor1 when _meta.FloatFuncCode.HasValue && _meta.FloatFormats is not null  // Floating-Point
                => FloatInstruction.Create(_meta.FloatFuncCode.Value, _format, (FloatRegister)_rs, (FloatRegister)_rd, (FloatRegister)_rt),
            OperationCode.Coprocessor1 => _meta.CoProc1RS.HasValue ?                                    // CoProc1
                FloatInstruction.Create(_meta.CoProc1RS.Value, _rt, (FloatRegister)_rs) :
                _ = ThrowHelper.ThrowArgumentException<MipsInstruction>($"Instruction with OpCode:{_meta.OpCode} must have a {nameof(_meta.CoProc1RS)} or {nameof(_meta.FloatFuncCode)} value."),

            // Register Immediate
            OperationCode.RegisterImmediate => _meta.RegisterImmediateFuncCode switch
            {
                // Register Immediate Branching
                (>= RegImmFuncCode.BranchOnLessThanZero and <= RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikely) or
                (>= RegImmFuncCode.BranchOnLessThanZeroAndLink and <= RegImmFuncCode.BranchOnGreaterThanOrEqualToZeroLikelyAndLink)
                    => MipsInstruction.Create(_meta.RegisterImmediateFuncCode.Value, _rs, _immediate),

                // Throw exception if null
                null => ThrowHelper.ThrowArgumentException<MipsInstruction>($"Instruction with OpCode:{_meta.OpCode} must have a {nameof(_meta.RegisterImmediateFuncCode)} value."),

                // Register Immediate
                _ => MipsInstruction.Create(_meta.RegisterImmediateFuncCode.Value, _rs, (short)_immediate)
            },

            // I-Type Branch
            (>= OperationCode.BranchOnEquals and <= OperationCode.BranchOnGreaterThanZero) or
            (>= OperationCode.BranchOnEqualLikely and <= OperationCode.BranchOnGreaterThanZeroLikely)
                    => MipsInstruction.Create(_meta.OpCode.Value, _rs, _rt, _immediate),

            // Remaining I Type instructions
            _ => MipsInstruction.Create(_meta.OpCode.Value, _rs, _rt, (short)_immediate),
        };
    }
}
