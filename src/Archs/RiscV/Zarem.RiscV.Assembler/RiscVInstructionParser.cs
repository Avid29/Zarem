// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using CommunityToolkit.HighPerformance;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Zarem.Assembler;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Assembler.Models.Tables;
using Zarem.Assembler.Parsers;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Assembler.Tokenization.Profiles;
using Zarem.Attributes.Arguments;
using Zarem.Models;
using Zarem.Models.Tables;
using Zarem.RiscV.Assembler.Logger;
using Zarem.RiscV.Assembler.Models.Enums;
using Zarem.RiscV.Assembler.Models.Meta;
using Zarem.RiscV.Assembler.Models.Tables;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums;
using Zarem.RiscV.Models.Instructions.Enums.Registers;
using Zarem.RiscV.Models.Versioning.Enums;

namespace Zarem.RiscV.Assembler;

/// <summary>
/// A struct for parsing RISC-V instructions.
/// </summary>
public class RiscVInstructionParser : InstructionParserBase<RiscVInstruction, RiscVInstructionMetaBase, RiscVArgument, RiscVGpRegister, RiscVRegisterSet>
{
    private readonly RiscVInstructionTable _instructionTable;
    private readonly AssemblerLogger? _logger;
    private readonly FormatTable<RiscVFloatFormat> _formatTable = new();

    private RiscVFloatFormat _format;

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVInstructionParser"/> struct.
    /// </summary>
    public RiscVInstructionParser(
        RiscVAssemblerConfig config,
        RiscVInstructionTable? table,
        Address address,
        IReadOnlyDictionary<string, Symbol>? symbols,
        ILogger? logger) : base(address, symbols, logger)
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
    protected override bool TryDetermineInstruction(AssemblyLine line, [NotNullWhen(true)] out string? name)
    {
        // Get instruction name and ensure it's not null
        name = line.Instruction?.Source;
        Guard.IsNotNull(line.Instruction);
        Guard.IsNotNull(name);

        // Parse out format from instruction name if present
        var parts = name.Split('.');
        for (int i = 1; i < parts.Length; i++)
        {
            if (_formatTable.TryGetFormat(parts[i], out var format))
            {
                _format = format;
                parts[i] = _formatTable.Placeholder;
            }
        }

        name = string.Join('.', parts).ToLowerInvariant();

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


        if (Meta is RiscVFloatInstructionMeta fMeta)
        {
            // Determine required extension based on the parsed format (.s, .d, .h, .q)
            RiscVExtensions formatRequirement = _format switch
            {
                RiscVFloatFormat.Single => RiscVExtensions.SingleFloatingPoint,
                RiscVFloatFormat.Double => RiscVExtensions.DoubleFloatingPoint,
                RiscVFloatFormat.Half => RiscVExtensions.HalfPrecisionFloatingPoint,
                RiscVFloatFormat.Quad => RiscVExtensions.QuadrupleFloatingPoint,
                _ => ThrowHelper.ThrowArgumentException<RiscVExtensions>(),
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

        // Set fixed values
        if (Meta.FixedRD.HasValue)ParsedArgTable[RiscVArgument.RD] = (RiscVGpRegister?)Meta.FixedRD;
        if (Meta.FixedRS1.HasValue)ParsedArgTable[RiscVArgument.RS1] = (RiscVGpRegister?)Meta.FixedRS1;
        if (Meta.FixedRS2.HasValue)ParsedArgTable[RiscVArgument.RS2] = (RiscVGpRegister?)Meta.FixedRS2;
        if (Meta.FixedRS3.HasValue)ParsedArgTable[RiscVArgument.FRS3] = (RiscVGpRegister?)Meta.FixedRS3;
        Immediate = Meta.FixedImm ?? default;

        return true;
    }

    /// <summary>
    /// Parses an argument as an expression and assigns it to the target component
    /// </summary>
    protected override bool TryParseExpression(ReadOnlySpan<Token> arg, RiscVArgument target, ImmediateArgumentAttribute attr)
    {
        if (!TryParseExpression(arg, attr.BitCount, attr.Signed, attr.ShiftAmount, out var expResult))
            return false;

        var requestedType = RiscVReferenceType.None;
        if (expResult.RelocationType is not null)
        {
            (requestedType, int bits) = expResult.RelocationType switch
            {
                "hi" => (RiscVReferenceType.High20, 20),
                "lo" => (RiscVReferenceType.Low12, 12),
                _ => ThrowHelper.ThrowArgumentOutOfRangeException<(RiscVReferenceType, int)>($"Relocation type '{expResult.RelocationType}' is not supported for RISC-V."),
            };

            if (attr.BitCount != bits)
            {
                _logger?.Log(Severity.Error, LogId.InvalidRelocationType, arg, "InvalidRelocationType", expResult.RelocationType, target);
                return false;
            }
        }

        var type = requestedType is RiscVReferenceType.None ? target switch
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
        } : requestedType;

        if (expResult.IsSymbolic && type is not RiscVReferenceType.None)
        {
            References.Add(new RelocationEntry(expResult.Symbol.Name, CurrentAddress, (uint)type, default));
        }
        else if (type is RiscVReferenceType.High20)
        {
            // TODO: Remove hacky solution to adjust offsets
            // on constants
            Immediate >>= 12;
        }

        return true;
    }

    /// <inheritdoc/>
    protected override RiscVInstruction BuildInstruction()
    {
        Guard.IsNotNull(Meta);

        var rd = GetParsedArgument<RiscVGpRegister>(RiscVArgument.RD, RiscVArgument.FRD);
        var rs1 = GetParsedArgument<RiscVGpRegister>(RiscVArgument.RS1, RiscVArgument.FRS1);
        var rs2 = GetParsedArgument<RiscVGpRegister>(RiscVArgument.RS2, RiscVArgument.FRS2);
        var rs3 = GetParsedArgument<RiscVGpRegister>(RiscVArgument.FRS3);

        return Meta switch
        {
            RTypeInstructionMeta r => RiscVInstruction.CreateR(r.OpCode, r.Funct3, r.Funct7, rd, rs1, rs2),
            ITypeInstructionMeta i => RiscVInstruction.CreateI(i.OpCode, i.Funct3, rd, rs1, (short)Immediate),
            UTypeInstructionMeta u => RiscVInstruction.CreateU(u.OpCode, rd, Immediate),
            BTypeInstructionMeta b => RiscVInstruction.CreateB(b.OpCode, b.Funct3, rs1, rs2, Immediate),
            STypeInstructionMeta s => RiscVInstruction.CreateS(s.OpCode, s.Funct3, rs1, rs2, (short)Immediate),
            JTypeInstructionMeta j => RiscVInstruction.CreateJ(j.OpCode, rd, Immediate),
            RiscVFloatInstructionMeta f => f.Funct5 is null
                ? RiscVFloatInstruction.Create(f.OpCode, _format, (RiscVFloatRegister)rd, (RiscVFloatRegister)rs1, (RiscVFloatRegister)rs2, (RiscVFloatRegister)rs3, f.Funct3)
                : RiscVFloatInstruction.Create(f.OpCode, _format, f.Funct5.Value, (RiscVFloatRegister)rd, (RiscVFloatRegister)rs1, (RiscVFloatRegister)rs2, f.Funct3),

            _ => throw new NotSupportedException($"Metadata type {Meta.GetType().Name} is not supported for encoding.")
        };
    }

    /// <inheritdoc/>
    protected override RiscVInstructionParser CreateSubParser(Address address)
        => new(Config, _instructionTable, address, Symbols, null);
}
