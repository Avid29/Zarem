// Avishai Dernis 2025

using CommunityToolkit.Diagnostics;
using CommunityToolkit.HighPerformance;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Zarem.Assembler;
using Zarem.Assembler.Helpers.Tables;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Assembler.Models.Tables;
using Zarem.Assembler.Parsers;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Assembler.Tokenization.Profiles;
using Zarem.Attributes;
using Zarem.Helpers;
using Zarem.Mips.Assembler.Logger;
using Zarem.Mips.Assembler.Models.Enums;
using Zarem.Mips.Assembler.Models.Meta;
using Zarem.Mips.Assembler.Models.Tables;
using Zarem.Mips.Extensions;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums;
using Zarem.Mips.Models.Instructions.Enums.Registers;
using Zarem.Models;
using Zarem.Models.Tables;

namespace Zarem.Mips.Assembler;

/// <summary>
/// A struct for parsing MIPS instructions.
/// </summary>
public class MipsInstructionParser : InstructionParserBase<MipsInstruction, MipsInstructionMetaBase, MipsArgument, MipsGpRegister, MipsRegisterSet>
{
    private readonly MipsInstructionTable _instructionTable;
    private readonly AssemblerLogger? _logger;
    private readonly FormatTable<MipsFloatFormat> _formatTable = new();

    private MipsGpRegister _rs;
    private MipsGpRegister _rt;
    private MipsGpRegister _rd;
    private MipsFloatFormat _format;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsInstructionParser"/> struct.
    /// </summary>
    public MipsInstructionParser(
        MipsAssemblerConfig config,
        MipsInstructionTable? table,
        Address address,
        IReadOnlyDictionary<string, Symbol>? symbols,
        ILogger? logger) : base(address, symbols, MipsRegisterTable.Instance, logger)
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
                not null when Config is null || version > Config.Version => (LogId.NotInVersion, "RequiresVersion"),
                not null => (LogId.NotInVersion, "RemovedInVersion"),
                null when Config is not null && is64bit && !Config.Version.Is64Bit() => (LogId.NotInVersion, "Needs64BitVersion"),
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
        _rs = (MipsGpRegister)(Meta.FixedRS ?? default);
        _rt = (MipsGpRegister)(Meta.FixedRT ?? default);
        _rd = (MipsGpRegister)(Meta.FixedRD ?? default);

        return true;
    }

    /// <inheritdoc/>
    protected override bool TryParseExpression(ReadOnlySpan<Token> arg, MipsArgument target, ImmediateArgumentAttribute attr)
    {
        if (!TryParseExpression(arg, attr.BitCount, attr.Signed, attr.ShiftAmount, out var expResult))
            return false;

        var requestedType = MipsReferenceType.None;
        if (expResult.RelocationType is not null)
        {
            requestedType = expResult.RelocationType switch
            {
                "hi" => MipsReferenceType.High16,
                "lo" => MipsReferenceType.Low16,
                "got" => MipsReferenceType.GlobalOffsetTable16,
                "call16" => MipsReferenceType.Call16,
                _ => ThrowHelper.ThrowArgumentOutOfRangeException<MipsReferenceType>($"Relocation type '{expResult.RelocationType}' is not supported for MIPS."),
            };

            if (attr.BitCount != 16)
            {
                _logger?.Log(Severity.Error, LogId.InvalidRelocationType, arg, "InvalidRelocationType", expResult.RelocationType, target);
                return false;
            }
        }

        // Determine the reference type based on the target argument type and requested relocation type
        var type = requestedType is MipsReferenceType.None ? target switch
        {
            MipsArgument.Address => MipsReferenceType.JumpTarget26,
            MipsArgument.Immediate => MipsReferenceType.Low16,
            MipsArgument.Offset => MipsReferenceType.PCRelative16,
            MipsArgument.LargeOffset => MipsReferenceType.PCRelative26,

            // FullImmediate is handled since it triggers a HI/LO pair
            // Which is handled in the child parser pass after expansion,
            // so we don't need to add a relocation entry here.
            MipsArgument.FullImmediate or _ => MipsReferenceType.None,
        } : requestedType;

        if (expResult.IsSymbolic && type is not MipsReferenceType.None)
        {
            References.Add(new RelocationEntry(expResult.Symbol.Name, CurrentAddress, (uint)type, default));
        }
        else if (type is MipsReferenceType.High16)
        {
            // TODO: Remove hacky solution to adjust offsets
            // on constants
            Immediate >>= 16;
        }

        return true;
    }

    /// <inheritdoc/>
    protected override bool TryParseRegister(ReadOnlySpan<Token> arg, MipsArgument target, RegisterArgumentAttribute<MipsRegisterSet> attr)
    {
        var set = attr.RegisterSet;

        // Get reference to selected register argument
        Ref<MipsGpRegister> regRef = target switch
        {
            // General Purpose Registers
            MipsArgument.RS => new(ref _rs),
            MipsArgument.RT => new(ref _rt),
            MipsArgument.RD => new(ref _rd),

            // Float Registers
            MipsArgument.FS => new(ref _rs),
            MipsArgument.FT => new(ref _rt),
            MipsArgument.FD => new(ref _rd),

            // RT Register for coprocessors
            MipsArgument.RT_Numbered => new(ref _rt),

            // Invalid target type
            _ => throw new ArgumentOutOfRangeException($"Argument of type '{target}' attempted to parse as a register.")
        };

        ref MipsGpRegister reg = ref regRef.Value;

        if (!TryParseRegister(arg, out var register, set, 32))
            return false;

        // Cache register as appropriate argument type
        reg = register;

        return true;
    }

    /// <inheritdoc/>
    protected override MipsInstruction BuildInstruction()
    {
        Guard.IsNotNull(Meta);

        return Meta switch
        {
            RTypeInstructionMeta r => MipsInstruction.CreateR(r.OperationCode, r.FuncCode, _rs, _rt, _rd, (byte)Immediate),
            JTypeInstructionMeta j => MipsInstruction.CreateJ(j.OperationCode, (uint)Immediate),

            RegImmInstructionMeta ri => ri.Type is MipsInstructionType.RegisterImmediateBranch
                ? MipsInstruction.CreateBranch(ri.RtCode, _rs, Immediate)
                : MipsInstruction.CreateTrap(ri.RtCode, _rs, (short)Immediate),

            CoProc0InstructionsMeta c0 when c0.Mfmc0FuncCode.HasValue => CoProc0Instruction.Create(c0.Mfmc0FuncCode.Value, _rt, (byte)_rd),
            CoProc0InstructionsMeta c0 when c0.FuncCode.HasValue => CoProc0Instruction.Create(c0.FuncCode.Value, _rd),
            CoProc0InstructionsMeta c0 => CoProc0Instruction.Create(c0.RSCode, _rt, _rd),

            CoProc1InstructionsMeta c1 => MipsFloatInstruction.Create(c1.RSCode, _rt, (MipsFloatRegister)_rs),
            MipsFloatInstructionMeta f => MipsFloatInstruction.Create(f.Function, _format, (MipsFloatRegister)_rs, (MipsFloatRegister)_rd, (MipsFloatRegister)_rt),

            ITypeInstructionMeta i => i.Type is MipsInstructionType.IBranch
            ? MipsInstruction.CreateBranch(i.OperationCode, _rs, _rt, Immediate)
            : MipsInstruction.CreateI(i.OperationCode, _rs, _rt, (short)Immediate),

            _ => throw new NotSupportedException($"Metadata type {Meta.GetType().Name} is not supported for encoding.")
        };
    }

    /// <inheritdoc/>
    protected override MipsInstructionParser CreateSubParser(Address address)
        => new(Config, _instructionTable, address, Symbols, null);
}
