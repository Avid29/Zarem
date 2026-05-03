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
using Zarem.Assembler.Models.Enums;
using Zarem.Assembler.Models.Meta;
using Zarem.Assembler.Models.Tables;
using Zarem.Assembler.Parsers;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Assembler.Tokenization.Profiles;
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
public class RiscVInstructionParser : InstructionParserBase<RiscVInstruction, RiscVInstructionMetaBase, RiscVArgument, RiscVGpRegister, RiscVRegisterSet>
{
    private readonly RiscVInstructionTable _instructionTable;
    private readonly AssemblerLogger? _logger;

    //private RiscVFloatFormat _format;
    private RiscVGpRegister _rd;
    private RiscVGpRegister _rs1;
    private RiscVGpRegister _rs2;

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

    /// <inheritdoc/>
    protected override ITokenizerProfile TemplateProfile { get; } = new RiscVTokenizerProfile();

    /// <inheritdoc/>
    protected override string GetTemplateArgSubstitution(RiscVArgument argType)
    {
        return argType switch
        {
            RiscVArgument.RD => RiscVRegisterTable.Instance.GetRegisterString(_rd, RiscVRegisterSet.GeneralPurpose),
            RiscVArgument.RS1 => RiscVRegisterTable.Instance.GetRegisterString(_rs1, RiscVRegisterSet.GeneralPurpose),
            RiscVArgument.RS2 => RiscVRegisterTable.Instance.GetRegisterString(_rs2, RiscVRegisterSet.GeneralPurpose),
            RiscVArgument.FRD => RiscVRegisterTable.Instance.GetRegisterString(_rd, RiscVRegisterSet.FloatingPoints),
            RiscVArgument.FRS1 => RiscVRegisterTable.Instance.GetRegisterString(_rs1, RiscVRegisterSet.FloatingPoints),
            RiscVArgument.FRS2 => RiscVRegisterTable.Instance.GetRegisterString(_rs2, RiscVRegisterSet.FloatingPoints),
            RiscVArgument.FRS3 => RiscVRegisterTable.Instance.GetRegisterString(_rs2, RiscVRegisterSet.FloatingPoints), // TODO

            RiscVArgument.Immediate or RiscVArgument.FullImmediate or RiscVArgument.BranchOffset or RiscVArgument.StoreOffset or
            RiscVArgument.JumpOffset or RiscVArgument.UpperImmediate or RiscVArgument.UImm5 => $"{Immediate}",

            RiscVArgument.MemoryLoad or RiscVArgument.MemoryStore => $"{Immediate}({RiscVRegisterTable.Instance.GetRegisterString(_rs1, RiscVRegisterSet.GeneralPurpose)})",

            RiscVArgument.Csr => "", // TODO

            _ => ThrowHelper.ThrowArgumentException<string>(),
        };
    }

    /// <inheritdoc/>
    protected override bool TryDetermineInstruction(AssemblyLine line, [NotNullWhen(true)] out string? name)
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

        Meta = metas.FirstOrDefault(x => x.ArgumentPattern.Length == line.Args.Count);

        if (Meta is null)
        {
            _logger?.Log(Severity.Error, LogId.InvalidInstructionArgCount, line.Instruction, "WrongArgumentCount", name, line.Args.Count);
            return false;
        }

        /*
        if (Meta is RiscVFloatInstructionMeta fMeta)
        {
            // Determine required extension based on the parsed format (.s, .d, .h, .q)
            RiscVExtensions formatRequirement = _format switch
            {
                RiscVFloatFormat.Single => RiscVExtensions.SingleFloatingPoint,
                RiscVFloatFormat.Double => RiscVExtensions.DoubleFloatingPoint,
                RiscVFloatFormat.Half => RiscVExtensions.HalfPrecisionFloatingPoint,
                RiscVFloatFormat.Quad => RiscVExtensions.QuadrupleFloatingPoint,
                _ => RiscVExtensions.Integers // Fallback
            };

            // Cross-reference with the Configured extensions
            if (!Config.VersionInfo.Extensions.HasFlag(formatRequirement))
            {
                _logger?.Log(Severity.Error,
                    LogId.NotInVersion,
                    line.Instruction,
                    "FormatRequiresExtension",
                    _format,
                    formatRequirement); // TODO: Improve message
                return false;
            }
        }
        */

        // Set fixed values
        _rs1 = (RiscVGpRegister)(Meta.FixedRS1 ?? default);
        _rs2 = (RiscVGpRegister)(Meta.FixedRS2 ?? default);
        _rd = (RiscVGpRegister)(Meta.FixedRD ?? default);
        Immediate = Meta.FixedImm ?? default;

        return true;
    }

    /// <inheritdoc/>
    protected override bool TryParseArg(ReadOnlySpan<Token> arg, RiscVArgument type)
    {
        return type switch
        {
            // Register arguments
            (>= RiscVArgument.RD and <= RiscVArgument.FRS3) => TryParseRegisterArg(arg, type),

            // Expression arguments
            (>= RiscVArgument.Immediate and <= RiscVArgument.FullImmediate) => TryParseExpressionArg(arg, type),

            // Address offset arguments
            RiscVArgument.MemoryLoad => TryParseAddressOffsetArg(arg, RiscVArgument.Immediate),
            RiscVArgument.MemoryStore => TryParseAddressOffsetArg(arg, RiscVArgument.StoreOffset),

            _ => ThrowHelper.ThrowArgumentOutOfRangeException<bool>($"Argument of type '{type}' is not within parsable type range."),
        };
    }

    /// <inheritdoc/>
    protected override RiscVInstruction BuildInstruction()
    {
        Guard.IsNotNull(Meta);

        return Meta switch
        {
            RTypeInstructionMeta r => RiscVInstruction.CreateR(r.OpCode, r.Funct3, r.Funct7, _rd, _rs1, _rs2),
            ITypeInstructionMeta i => RiscVInstruction.CreateI(i.OpCode, i.Funct3, _rd, _rs1, (short)Immediate),
            UTypeInstructionMeta u => RiscVInstruction.CreateU(u.OpCode, _rd, Immediate),
            BTypeInstructionMeta b => RiscVInstruction.CreateB(b.OpCode, b.Funct3, _rs1, _rs2, Immediate),
            STypeInstructionMeta s => RiscVInstruction.CreateS(s.OpCode, s.Funct3, _rs1, _rs2, (short)Immediate),
            JTypeInstructionMeta j => RiscVInstruction.CreateJ(j.OpCode, _rd, Immediate),

            _ => throw new NotSupportedException($"Metadata type {Meta.GetType().Name} is not supported for encoding.")
        };
    }

    /// <inheritdoc/>
    protected override RiscVInstructionParser CreateSubParser()
        => new(Config, _instructionTable, CurrentAddress, null, null);

    /// <summary>
    /// Parses an argument as a register and assigns it to the target component.
    /// </summary>
    private bool TryParseRegisterArg(ReadOnlySpan<Token> arg, RiscVArgument target)
    {
        // Get reference to selected register argument
        RefTuple<Ref<RiscVGpRegister>, RiscVRegisterSet> pair = target switch
        {
            // General Purpose Registers
            RiscVArgument.RD => new(new(ref _rd), RiscVRegisterSet.GeneralPurpose),
            RiscVArgument.RS1 => new(new(ref _rs1), RiscVRegisterSet.GeneralPurpose),
            RiscVArgument.RS2 => new(new(ref _rs2), RiscVRegisterSet.GeneralPurpose),

            // Float Registers
            RiscVArgument.FRD => new(new(ref _rd), RiscVRegisterSet.FloatingPoints),
            RiscVArgument.FRS1 => new(new(ref _rs1), RiscVRegisterSet.FloatingPoints),
            RiscVArgument.FRS2 => new(new(ref _rs2), RiscVRegisterSet.FloatingPoints),

            // Invalid target type
            _ => throw new ArgumentOutOfRangeException($"Argument of type '{target}' attempted to parse as a register.")
        };

        (Ref<RiscVGpRegister> regRef, RiscVRegisterSet set) = pair;
        ref RiscVGpRegister reg = ref regRef.Value;

        if (!TryParseRegister(arg, out var register, set, 32))
            return false;

        // Cache register as appropriate argument type
        reg = register;

        return true;
    }

    /// <summary>
    /// Parses an argument as an expression and assigns it to the target component
    /// </summary>
    private bool TryParseExpressionArg(ReadOnlySpan<Token> arg, RiscVArgument target)
    {
        var type = target switch
        {
            RiscVArgument.JumpOffset => RiscVReferenceType.Jump20,
            RiscVArgument.BranchOffset => RiscVReferenceType.Branch20,
            RiscVArgument.Immediate => RiscVReferenceType.Low12,
            RiscVArgument.UpperImmediate => RiscVReferenceType.High20,
            // 'Memory' in RISC-V loads/stores uses a 12-bit offset (%lo)
            RiscVArgument.StoreOffset => RiscVReferenceType.Low12,
            // FullImmediate triggers a HI/LO pair
            RiscVArgument.FullImmediate => RiscVReferenceType.High20,   
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<RiscVReferenceType>($"Argument of type '{target}' cannot reference relocatable symbols."),
        };

        // Determine casting details for the RISC-V argument
        (int bitCount, int shiftAmount, bool signed) = target switch
        {
            // 5-bit unsigned immediate (e.g., vsetvli or CSRI)
            RiscVArgument.UImm5 => (5, 1, false),
            RiscVArgument.Immediate or
            RiscVArgument.StoreOffset => (12, 0, true),
            RiscVArgument.BranchOffset => (12, 1, true),
            RiscVArgument.JumpOffset => (20, 1, true),
            RiscVArgument.UpperImmediate => (20, 0, false),
            RiscVArgument.Csr => (12, 0, false),
            RiscVArgument.FullImmediate => (32, 0, true),

            _ => ThrowHelper.ThrowArgumentOutOfRangeException<(int, int, bool)>(
                $"Argument of type '{target}' attempted to parse as an expression.")
        };

        if (!TryParseExpression(arg, bitCount, shiftAmount, signed, out var expResult))
            return false;

        if (expResult.IsSymbolic)
        {
            if (target is RiscVArgument.FullImmediate)
            {
                References.Add(new RelocationEntry(expResult.Symbol.Name, CurrentAddress, (uint)RiscVReferenceType.High20, default));
                References.Add(new RelocationEntry(expResult.Symbol.Name, CurrentAddress + 4, (uint)RiscVReferenceType.Low12, default));
            }
            else
            {
                References.Add(new RelocationEntry(expResult.Symbol.Name, CurrentAddress, (uint)type, default));
            }
            
        }

        return true;
    }

    /// <summary>
    /// Parses an argument as an address offset, assigning its components to immediate and $rs.
    /// </summary>
    private bool TryParseAddressOffsetArg(ReadOnlySpan<Token> arg, RiscVArgument immType)
    {
        // NOTE: Be careful about forwards to other parse functions with regards to 
        // error logging. Address offset argument errors might be inappropriately logged.

        // Split the string into an offset and a register, return false if failed
        if (!SplitOffsetBase(arg, out var offsetStr, out var regStr))
            return false;

        // Try parse offset component into immediate, return false if failed
        if (!TryParseExpressionArg(offsetStr, immType))
            return false;

        // Parse register component into $rs, return false if failed
        if (!TryParseRegisterArg(regStr, RiscVArgument.RS1))
            return false;

        return true;
    }
}
