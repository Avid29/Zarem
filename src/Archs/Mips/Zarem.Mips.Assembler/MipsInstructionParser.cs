// Avishai Dernis 2025

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
using Zarem.Mips.Assembler.Logger;
using Zarem.Mips.Assembler.Models.Meta;
using Zarem.Mips.Assembler.Models.Tables;
using Zarem.Mips.Models.Enums;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums;
using Zarem.Mips.Models.Instructions.Enums.Registers;
using Zarem.Models;
using Zarem.Models.Tables;

namespace Zarem.Mips.Assembler;

/// <summary>
/// A struct for parsing MIPS instructions.
/// </summary>
public class MipsInstructionParser : InstructionParserBase<MipsInstruction, MipsInstructionMetaBase, MipsArgument, MipsGpRegister, MipsRegisterSet, MipsReferenceType>
{
    private readonly MipsInstructionTable _instructionTable;
    private readonly AssemblerLogger? _logger;
    private readonly FormatTable<MipsFloatFormat> _formatTable = new();

    private MipsFloatFormat _format;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsInstructionParser"/> struct.
    /// </summary>
    public MipsInstructionParser(
        MipsAssemblerConfig config,
        MipsInstructionTable? table,
        Address address,
        IReadOnlyDictionary<string, Symbol>? symbols,
        ILogger? logger) : base(address, symbols, logger)
    {
        Config = config;

        _instructionTable = table ?? new MipsInstructionTable(config);

        if (logger is not null)
        {
            _logger = new AssemblerLogger(logger);
        }
    }

    /// <inheritdoc/>
    protected override MipsAssemblerConfig Config { get; }

    /// <inheritdoc/>
    protected override ITokenizerProfile TemplateProfile { get; } = new MipsTokenizerProfile();

    /// <inheritdoc/>
    protected override bool TryDetermineInstruction(AssemblyLine line, [NotNullWhen(true)] out string? name)
    {
        // Get instruction name and ensure it's not null
        name = line.Instruction?.Source;
        Guard.IsNotNull(line.Instruction);
        Guard.IsNotNull(name);

        // Parse out format from instruction name if present
        var parts = name.Split('.');
        if (_formatTable.TryGetFormat(parts[^1], out var format))
        {
            _format = format;
            parts[^1] = _formatTable.Placeholder;
        }

        name = string.Join('.', parts);

        if (!_instructionTable.TryGetInstruction(name, out var metas, out var version, out var is64bit, out var banned))
        {
            (LogId id, string message) = version switch
            {
                not null when banned => (LogId.DisabledFeatureInUse, "InstructionDisabled"),
                not null when Config is null || version > Config.VersionInfo.Base => (LogId.NotInVersion, "RequiresVersion"),
                not null => (LogId.NotInVersion, "RemovedInVersion"),
                null when Config is not null && is64bit && !Config.VersionInfo.Is64Bit => (LogId.NotInVersion, "Needs64BitVersion"),
                null => (LogId.InvalidInstructionName, "NoInstructionNamed")
            };

            _logger?.Log(Severity.Error, id, line.Instruction, message, name, $"{version:d}");
            return false;
        }

        Meta = metas.FirstOrDefault(x => x.ArgumentCount == line.Args.Count);

        if (Meta is null)
        {
            _logger?.Log(Severity.Error, LogId.InvalidInstructionArgCount, line.Instruction, "WrongArgumentCount", name, line.Args.Count);
            return false;
        }

        // Check float format support via the specialized Float record
        if (Meta is MipsFloatInstructionMeta fMeta && fMeta.SupportedFormats is not null && !fMeta.SupportedFormats.Contains(_format))
        {
            _logger?.Log(Severity.Error, LogId.InvalidFloatFormat, line.Instruction, $"DoesNotSupportFormat{_format}", name);
            return false;
        }

        // Set fixed values
        if (Meta.FixedRS.HasValue) ParsedArgTable[MipsArgument.RS] = (MipsGpRegister?)Meta.FixedRS;
        if (Meta.FixedRT.HasValue) ParsedArgTable[MipsArgument.RT] = (MipsGpRegister?)Meta.FixedRT;
        if (Meta.FixedRD.HasValue) ParsedArgTable[MipsArgument.RD] = (MipsGpRegister?)Meta.FixedRD;

        return true;
    }

    /// <inheritdoc/>
    protected override MipsInstruction BuildInstruction()
    {
        Guard.IsNotNull(Meta);

        var rs = GetParsedArgument<MipsGpRegister>(MipsArgument.RS, MipsArgument.FS);
        var rt = GetParsedArgument<MipsGpRegister>(MipsArgument.RT, MipsArgument.FT);
        var rd = GetParsedArgument<MipsGpRegister>(MipsArgument.RD, MipsArgument.FD);

        return Meta switch
        {
            RTypeInstructionMeta r => MipsInstruction.CreateR(r.OperationCode, r.FuncCode, rs, rt, rd, (byte)Immediate),
            JTypeInstructionMeta j => MipsInstruction.CreateJ(j.OperationCode, (uint)Immediate),

            RegImmInstructionMeta ri => ri.Type is MipsInstructionType.RegisterImmediateBranch
                ? MipsInstruction.CreateBranch(ri.RtCode, rs, Immediate)
                : MipsInstruction.CreateTrap(ri.RtCode, rs, (short)Immediate),

            CoProc0InstructionsMeta c0 when c0.Mfmc0FuncCode.HasValue => CoProc0Instruction.Create(c0.Mfmc0FuncCode.Value, rt, (byte)rd),
            CoProc0InstructionsMeta c0 when c0.FuncCode.HasValue => CoProc0Instruction.Create(c0.FuncCode.Value, rd),
            CoProc0InstructionsMeta c0 => CoProc0Instruction.Create(c0.RSCode, rt, rd),

            CoProc1InstructionsMeta c1 => MipsFloatInstruction.Create(c1.RSCode, rt, (MipsFloatRegister)rs),
            MipsFloatInstructionMeta f => MipsFloatInstruction.Create(f.Function, _format, (MipsFloatRegister)rs, (MipsFloatRegister)rd, (MipsFloatRegister)rt),

            ITypeInstructionMeta i => i.Type is MipsInstructionType.IBranch
            ? MipsInstruction.CreateBranch(i.OperationCode, rs, rt, Immediate)
            : MipsInstruction.CreateI(i.OperationCode, rs, rt, (short)Immediate),

            _ => throw new NotSupportedException($"Metadata type {Meta.GetType().Name} is not supported for encoding.")
        };
    }

    /// <inheritdoc/>
    protected override MipsInstructionParser CreateSubParser(Address address)
        => new(Config, _instructionTable, address, Symbols, null);
}
