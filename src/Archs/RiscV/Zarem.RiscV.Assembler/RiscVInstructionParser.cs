// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
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
using Zarem.Models;
using Zarem.Models.Tables;
using Zarem.RiscV.Assembler.Logger;
using Zarem.RiscV.Assembler.Models.Meta;
using Zarem.RiscV.Assembler.Models.Meta.Extensions;
using Zarem.RiscV.Assembler.Models.Meta.Extensions.Compressed;
using Zarem.RiscV.Assembler.Models.Tables;
using Zarem.RiscV.Models.Enums;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums;
using Zarem.RiscV.Models.Instructions.Enums.Registers;
using Zarem.RiscV.Models.Versioning.Enums;

namespace Zarem.RiscV.Assembler;

/// <summary>
/// A struct for parsing RISC-V instructions.
/// </summary>
public class RiscVInstructionParser : InstructionParserBase<RiscVInstruction, RiscVInstructionMetaBase, RiscVArgument, RiscVGpRegister, RiscVRegisterSet, RiscVReferenceType>
{
    private readonly RiscVInstructionTable _instructionTable;
    private readonly AssemblerLogger? _logger;
    private readonly FormatTable<RiscVFloatFormat> _formatTable = new();
    private readonly FormatTable<RiscVRoundingMode> _roundingModeTable = new("rm");

    private RiscVFloatFormat _format;
    private RiscVRoundingMode _roundingMode;

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

        // Parse out format/rounding_mode from instruction name if present
        var parts = name.Split('.');
        for (int i = 1; i < parts.Length; i++)
        {
            if (_formatTable.TryGetFormat(parts[i], out var format))
            {
                _format = format;
                parts[i] = _formatTable.Placeholder;
            }
            else if (_roundingModeTable.TryGetFormat(parts[i], out var roundingMode))
            {
                _roundingMode = roundingMode;
                parts[i] = _roundingModeTable.Placeholder;
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

    /// <inheritdoc/>
    protected override RiscVInstruction BuildInstruction()
    {
        Guard.IsNotNull(Meta);

        var rd = GetParsedArgument<RiscVGpRegister>(RiscVArgument.RD, RiscVArgument.FRD, RiscVArgument.RDRS1, RiscVArgument.CompressedRDRS1);
        var rs1 = GetParsedArgument<RiscVGpRegister>(RiscVArgument.RS1, RiscVArgument.FRS1, RiscVArgument.RDRS1, RiscVArgument.CompressedRS1, RiscVArgument.CompressedRDRS1);
        var rs2 = GetParsedArgument<RiscVGpRegister>(RiscVArgument.RS2, RiscVArgument.FRS2, RiscVArgument.CompressedRS2);
        var rs3 = GetParsedArgument<RiscVGpRegister>(RiscVArgument.FRS3);

        return Meta switch
        {
            RTypeInstructionMeta r => RiscVInstruction.CreateR(r.OpCode, r.Funct3, r.Funct7, rd, rs1, rs2),
            ITypeInstructionMeta i => RiscVInstruction.CreateI(i.OpCode, i.Funct3, rd, rs1, (short)Immediate),
            UTypeInstructionMeta u => RiscVInstruction.CreateU(u.OpCode, rd, Immediate),
            BTypeInstructionMeta b => RiscVInstruction.CreateB(b.OpCode, b.Funct3, rs1, rs2, Immediate),
            STypeInstructionMeta s => RiscVInstruction.CreateS(s.OpCode, s.Funct3, rs1, rs2, (short)Immediate),
            JTypeInstructionMeta j => RiscVInstruction.CreateJ(j.OpCode, rd, Immediate),
            CBTypeInstructionMeta cb => RiscVCompressedInstruction.CreateCB(cb.CompressionCode, cb.CFunct3, rs1, (short)Immediate),
            CITypeInstructionMeta ci => RiscVCompressedInstruction.CreateCI(ci.CompressionCode, ci.CFunct3, rd, (sbyte)Immediate),
            CRTypeInstructionMeta cr => RiscVCompressedInstruction.CreateCR(cr.CompressionCode, cr.CFunct4, rd, rs2),
            RiscVFloatInstructionMeta f => (f.Funct5.HasValue, f.Funct3.HasValue) switch
            {
                // Triple Source reg with rounding mode
                (false, false) => RiscVFloatInstruction.Create(f.OpCode, _format, (RiscVFloatRegister)rd, (RiscVFloatRegister)rs1, (RiscVFloatRegister)rs2, (RiscVFloatRegister)rs3, _roundingMode),

                // Triple source reg without rounding mode
                (false, true) => RiscVFloatInstruction.Create(f.OpCode, _format, (RiscVFloatRegister)rd, (RiscVFloatRegister)rs1, (RiscVFloatRegister)rs2, (RiscVFloatRegister)rs3, f.Funct3!.Value),

                // Double source reg with rounding mode
                (true, false) => RiscVFloatInstruction.Create(f.OpCode, _format, f.Funct5!.Value, (RiscVFloatRegister)rd, (RiscVFloatRegister)rs1, (RiscVFloatRegister)rs2, _roundingMode),

                // Double source reg without rounding mode
                (true, true) => RiscVFloatInstruction.Create(f.OpCode, _format, f.Funct5!.Value, (RiscVFloatRegister)rd, (RiscVFloatRegister)rs1, (RiscVFloatRegister)rs2, f.Funct3!.Value),
            },
            _ => throw new NotSupportedException($"Metadata type {Meta.GetType().Name} is not supported for encoding.")
        };
    }

    /// <inheritdoc/>
    protected override RiscVInstructionParser CreateSubParser(Address address)
        => new(Config, _instructionTable, address, Symbols, null);
}
