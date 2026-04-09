// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using CommunityToolkit.HighPerformance;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Zarem.Assembler.Helpers.Tables;
using Zarem.Assembler.Logger;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Assembler.Models;
using Zarem.Assembler.Models.Abstract;
using Zarem.Assembler.Models.Enums;
using Zarem.Assembler.Models.Meta;
using Zarem.Assembler.Parsers;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Helpers;
using Zarem.Models;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Tables;

namespace Zarem.Assembler;


/// <summary>
/// A struct for parsing RISC-V instructions.
/// </summary>
public class RiscVInstructionParser : InstructionParserBase<GPRegister, RegisterSet>
{
    private readonly RiscVInstructionTable _instructionTable;
    private readonly AssemblerLogger? _logger;

    private RiscVInstructionMetaBase? _meta;

    private GPRegister _rd;
    private GPRegister _rs1;
    private GPRegister _rs2;

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVInstructionParser"/> struct.
    /// </summary>
    public RiscVInstructionParser(
        RiscVAssemblerConfig config,
        RiscVInstructionTable? table,
        Address address,
        IReadOnlyDictionary<string, Symbol>? symbols,
        ILogger? logger) : base(address, symbols, RiscVRegisterTable.Instance, logger)
    {
        Config = config;

        _instructionTable = table ?? new RiscVInstructionTable(config);

        if (logger is not null)
        {
            _logger = new AssemblerLogger(logger);
        }
    }

    /// <inheritdoc/>
    protected override RiscVAssemblerConfig Config { get; }

    /// <summary>
    /// Attempts to parse an instruction from a name and a list of arguments.
    /// </summary>
    /// <param name="line">The assembly line to parse.</param>
    /// <returns>The parsed instruction.</returns>
    public RiscVParsedInstruction? Parse(AssemblyLine line)
    {
        // Attempt to load the instruction
        // If successful, this will set the _meta and _format
        if (!TryParseInstruction(line, out var name))
            return null;

        // Applies provided values
        _rs1 = (GPRegister)(_meta.FixedRS1 ?? default);
        _rs2 = (GPRegister)(_meta.FixedRS2 ?? default);
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

        // Build an instruction using the information from
        // _meta and all the parsed arguments
        var instruction = BuildInstruction();
        return new RiscVParsedInstruction(instruction, References);
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
            Argument.Memory => TryParseAddressOffsetArg(arg),

            _ => ThrowHelper.ThrowArgumentOutOfRangeException<bool>($"Argument of type '{type}' is not within parsable type range."),
        };
    }

    /// <summary>
    /// Parses an argument as a register and assigns it to the target component.
    /// </summary>
    private bool TryParseRegisterArg(ReadOnlySpan<Token> arg, Argument target)
    {
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

        if (!TryParseRegister(arg, out var register, set, 32))
            return false;

        // Cache register as appropriate argument type
        reg = register;

        return true;
    }

    /// <summary>
    /// Parses an argument as an expression and assigns it to the target component
    /// </summary>
    private bool TryParseExpressionArg(ReadOnlySpan<Token> arg, Argument target)
    {
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

        // Determine casting details for the RISC-V argument
        (int bitCount, int shiftAmount, bool signed) = target switch
        {
            // 5-bit unsigned immediate (e.g., vsetvli or CSRI)
            Argument.UImm5 => (5, 1, false),
            Argument.Immediate or Argument.StoreOffset or
            Argument.Memory => (12, 0, true),
            Argument.BranchOffset => (12, 1, true),
            Argument.JumpOffset => (20, 1, true),
            Argument.UpperImmediate => (20, 0, false),
            Argument.Csr => (12, 0, false),

            _ => ThrowHelper.ThrowArgumentOutOfRangeException<(int, int, bool)>(
                $"Argument of type '{target}' attempted to parse as an expression.")
        };

        if (!TryParseExpression(arg, bitCount, shiftAmount, signed, out var expResult))
            return false;

        if (expResult.IsSymbolic)
        {
            References.Add(new RelocationEntry(expResult.Symbol.Name, CurrentAddress, (uint)type, default));
        }

        return true;
    }

    /// <summary>
    /// Parses an argument as an address offset, assigning its components to immediate and $rs.
    /// </summary>
    private bool TryParseAddressOffsetArg(ReadOnlySpan<Token> arg)
    {
        // NOTE: Be careful about forwards to other parse functions with regards to 
        // error logging. Address offset argument errors might be inappropriately logged.

        // Split the string into an offset and a register, return false if failed
        if (!SplitOffsetBase(arg, out var offsetStr, out var regStr))
            return false;

        // Try parse offset component into immediate, return false if failed
        if (!TryParseExpressionArg(offsetStr, Argument.Immediate))
            return false;

        // Parse register component into $rs, return false if failed
        if (!TryParseRegisterArg(regStr, Argument.RS1))
            return false;

        return true;
    }

    private RiscVInstruction BuildInstruction()
    {
        Guard.IsNotNull(_meta);

        return _meta switch
        {
            RTypeInstructionMeta r => RiscVInstruction.CreateR(r.OpCode, r.Funct3, r.Funct7, _rd, _rs1, _rs2),
            ITypeInstructionMeta i => RiscVInstruction.CreateI(i.OpCode, i.Funct3, _rd, _rs1, (short)Immediate),
            UTypeInstructionMeta u => RiscVInstruction.CreateU(u.OpCode, _rd, Immediate),
            BTypeInstructionMeta b => RiscVInstruction.CreateB(b.OpCode, b.Funct3, _rs1, _rs2, Immediate),
            STypeInstructionMeta s => RiscVInstruction.CreateS(s.OpCode, s.Funct3, _rs1, _rs2, (short)Immediate),
            JTypeInstructionMeta j => RiscVInstruction.CreateJ(j.OpCode, _rd, Immediate),

            _ => throw new NotSupportedException($"Metadata type {_meta.GetType().Name} is not supported for encoding.")
        };
    }
}
