// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using CommunityToolkit.HighPerformance;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Zarem.Assembler.Extensions;
using Zarem.Assembler.Extensions.System;
using Zarem.Assembler.Helpers.Tables;
using Zarem.Assembler.Logger;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Assembler.Models;
using Zarem.Assembler.Models.Abstract;
using Zarem.Assembler.Models.Enums;
using Zarem.Assembler.Parsers;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Helpers;
using Zarem.Models;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Tables;
using Zarem.Models.Tables.Enums;

namespace Zarem.Assembler;


/// <summary>
/// A struct for parsing RISC-V instructions.
/// </summary>
public class RiscVInstructionParser : InstructionParserBase<GPRegister, RegisterSet>
{
    private readonly Address _currentAddress;
    private readonly IReadOnlyDictionary<string, Symbol>? _symbols;
    private readonly RiscVInstructionTable _instructionTable;
    private readonly AssemblerLogger? _logger;

    private RiscVInstructionMetaBase? _meta;

    private GPRegister _rd;
    private GPRegister _rs1;
    private GPRegister _rs2;
    private int _immediate;
    private List<RelocationEntry>? _references;

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVInstructionParser"/> struct.
    /// </summary>
    public RiscVInstructionParser(
        RiscVAssemblerConfig config,
        RiscVInstructionTable? table,
        Address address,
        IReadOnlyDictionary<string, Symbol>? symbols,
        ILogger? logger) : base(RiscVRegisterTable.Instance, logger)
    {
        Config = config;

        _currentAddress = address;
        _symbols = symbols;
        _references = [];

        _instructionTable = table ?? new RiscVInstructionTable(config);

        if (logger is not null)
        {
            _logger = new AssemblerLogger(logger);
        }
    }

    /// <inheritdoc/>
    public override RiscVAssemblerConfig Config { get; }

    /// <summary>
    /// Attempts to parse an instruction from a name and a list of arguments.
    /// </summary>
    /// <param name="line">The assembly line to parse.</param>
    /// <returns>The parser instruction.</returns>
    public void Parse(AssemblyLine line)
    {
        // Attempt to load the instruction
        // If successful, this will set the _meta and _format
        if (!TryParseInstruction(line, out var name))
            return;

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
    }

    [MemberNotNullWhen(true, nameof(_meta))]
    private bool TryParseInstruction(AssemblyLine line, [NotNullWhen(true)] out string? name)
    {
        // Get instruction name and ensure it's not null
        name = line.Instruction?.Source;
        Guard.IsNotNull(line.Instruction);
        Guard.IsNotNull(name);

        if (!_instructionTable.TryGetInstruction(name, out var metas, out var requiredBase, out var requiredExtension))
        {
            (LogId id, string message) = requiredExtension switch
            {
                not null => (LogId.NotInVersion, "RequiresExtension"),
                null when requiredBase is not null => (LogId.NotInVersion, "RequiresVersion"),
                null => (LogId.InvalidInstructionName, "NoInstructionNamed")
            };

            _logger?.Log(Severity.Error, id, line.Instruction, message, name, $"{requiredBase:d}", $"{requiredExtension:d}");
            return false;
        }

        _meta = metas.FirstOrDefault(x => x.ArgumentPattern.Length == line.Args.Count);

        if (_meta is null)
        {
            _logger?.Log(Severity.Error, LogId.InvalidInstructionArgCount, line.Instruction, "WrongArgumentCount", name, line.Args.Count);
            return false;
        }

        return true;
    }

    private bool TryParseArg(ReadOnlySpan<Token> arg, Argument type)
    {
        return type switch
        {
            // Register arguments
            (>= Argument.RD and <= Argument.FRS3) => TryParseRegisterArg(arg, type),

            // Expression arguments
            (>= Argument.Immediate and <= Argument.JumpOffset) => TryParseExpressionArg(arg, type),

            //// Address offset arguments
            //Argument.AddressBase => TryParseAddressOffsetArg(arg),

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
            Argument.RD => new(new(ref _rd), RegisterSet.GeneralPurpose),
            Argument.RS1 => new(new(ref _rs1), RegisterSet.GeneralPurpose),
            Argument.RS2 => new(new(ref _rs2), RegisterSet.GeneralPurpose),
            // Float Registers
            Argument.FRD => new(new(ref _rd), RegisterSet.FloatingPoints),
            Argument.FRS1 => new(new(ref _rs1), RegisterSet.FloatingPoints),
            Argument.FRS2 => new(new(ref _rs2), RegisterSet.FloatingPoints),

            // Invalid target type
            _ => throw new ArgumentOutOfRangeException($"Argument of type '{target}' attempted to parse as a register.")
        };

        (Ref<GPRegister> regRef, RegisterSet set) = pair;
        ref GPRegister reg = ref regRef.Value;

        if (!TryParseRegister(arg[0], out var register, set, 32))
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

        if (expResult.IsSymbolic)
        {
            // NOTE: If it were possible to have an undeclared local system reference,
            // it would be here. However, since there's not forward declaration of local
            // symbols, that's not a consider
            // Map RISC-V target arguments to the specific relocation types

            var type = target switch
            {
                Argument.JumpOffset => RiscVReferenceType.Jump20,
                Argument.BranchOffset => RiscVReferenceType.Branch20,
                Argument.Immediate => RiscVReferenceType.Low12,
                Argument.UpperImmediate => RiscVReferenceType.High20,
                // 'Memory' in RISC-V loads/stores uses a 12-bit offset (%lo)
                Argument.StoreOffset or Argument.Memory => RiscVReferenceType.Low12,
                _ => ThrowHelper.ThrowArgumentOutOfRangeException<RiscVReferenceType>($"Argument of type '{target}' cannot reference relocatable symbols."),
            };

            _references ??= [];
            var symbol = expResult.Symbol;
            var addend = (uint)expResult.Addend;

            _references.Add(new RelocationEntry(symbol.Name, _currentAddress, (uint)type, addend));
        }

        // NOTE: Casting might truncate the value to fit the bit size.
        // This is the desired behavior, but when logging errors this
        // should be handled explicitly and drop an assembler warning.
        //
        // ALSO NOTE: The linker will fill in the added if the value
        // is symbolic (not absolute)

        long value = expResult.IsAbsolute ? expResult.Addend : 0;

        // Determine casting details for the RISC-V argument
        (int bitCount, int shiftAmount, bool signed) = target switch
        {
            // 5-bit unsigned immediate (e.g., vsetvli or CSRI)
            Argument.UImm5 => (5, 1, false),

            // 12-bit signed immediate (I-type, S-type, and Load/Store offsets)
            Argument.Immediate or
            Argument.StoreOffset or
            Argument.Memory => (12, 0, true),

            // 12-bit signed branch offset (B-type)
            // Range is 13 bits total, but bit 0 is omitted (shifted by 1)
            Argument.BranchOffset => (12, 1, true),

            // 20-bit signed jump offset (J-type / JAL)
            // Range is 21 bits total, bit 0 omitted (shifted by 1)
            Argument.JumpOffset => (20, 1, true),

            // 20-bit unsigned upper immediate (U-type / LUI / AUIPC)
            // These are logically shifted left by 12 in the hardware, 
            // but the instruction carries the 20-bit raw value.
            Argument.UpperImmediate => (20, 0, false),

            // 12-bit CSR address (usually treated as an unsigned immediate)
            Argument.Csr => (12, 0, false),

            _ => ThrowHelper.ThrowArgumentOutOfRangeException<(int, int, bool)>(
                $"Argument of type '{target}' attempted to parse as an expression.")
        };

        // Truncates the value to fit the target argument
        CleanInteger(ref value, arg, bitCount, shiftAmount, signed);
        _immediate = (int)value;
        return true;
    }
}
