// Avishai Dernis 2026

using Zarem.Assembler.Logging;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Assembler.Models.Enums;
using Zarem.Extensions.System.IO;
using Zarem.Linker.Config;
using Zarem.Linker.Handlers;
using Zarem.Linker.Logging;
using Zarem.Models.Instructions;
using Zarem.Models.Tables;

namespace Zarem.Linker;

/// <summary>
/// A linker handler for the MIPS architecture.
/// </summary>
public class MipsLinkerHandler : ILinkerHandler<MipsLinkerConfig>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MipsLinkerHandler"/> class.
    /// </summary>
    public MipsLinkerHandler(MipsLinkerConfig config)
    {
        Config = config;
    }

    /// <inheritdoc/>
    public string GetArchitectureName() => "MIPS";

    /// <inheritdoc/>
    public MipsLinkerConfig Config { get; }

    /// <inheritdoc/>
    public bool PatchRelocation(Section section, RelocationEntry relocation, ulong offset, ulong symbolVirtual, ulong patchVirtual, ILogger logger)
    {
        var localLogger = new LinkerLogger(logger);

        section.Position = (long)offset;
        section.Stream.TryRead<uint>(out var value);

        var instruction = (MipsInstruction)value;

        long target = (long)symbolVirtual + relocation.Addend;
        long relativeTarget = target - ((long)patchVirtual + 4);

        value = (MipsReferenceType)relocation.Type switch
        {
            MipsReferenceType.Low16 => MIPS_Low16(instruction, target),
            MipsReferenceType.High16 => MIPS_High16(instruction, target),
            MipsReferenceType.PCRelative16 => MIPS_PC16(instruction, relativeTarget, relocation, localLogger),
            MipsReferenceType.JumpTarget26 => MIPS_Jump26(instruction, target, patchVirtual, relocation, localLogger),
            MipsReferenceType.Absolute32 => (uint)target,
            _ => Invalid_Type(value, relocation.Type, localLogger)
        };

        section.Position -= sizeof(uint);
        section.Stream.TryWrite(value);
        return true;
    }

    private static uint MIPS_Low16(MipsInstruction instruction, long target)
    {
        instruction.Immediate = (short)(target & 0xFFFF);
        return (uint)instruction;
    }

    private static uint MIPS_High16(MipsInstruction instruction, long target)
    {
        instruction.Immediate = (short)((target + 0x8000) >> 16);
        return (uint)instruction;
    }

    private static uint MIPS_PC16(MipsInstruction instruction, long relativeTarget, RelocationEntry relocation, LinkerLogger logger)
    {
        if (relativeTarget > 0xFFFF00 || relativeTarget < -0xFFFF00)
        {
            logger.Log(Severity.Error, LogId.OutOfRange, "TODO: Get file name", "BranchOutOfRange", relocation.SymbolName);
        }

        instruction.Offset = (int)relativeTarget;
        return (uint)instruction;
    }

    private static uint MIPS_Jump26(MipsInstruction instruction, long target, ulong patchVirtual, RelocationEntry relocation, LinkerLogger logger)
    {
        if (((ulong)target & 0xF0000000UL) != (patchVirtual & 0xF0000000UL))
        {
            logger.Log(Severity.Error, LogId.OutOfRange, "TODO: Get file name", "JumpOutOfRange", relocation.SymbolName);
        }

        instruction.Address = (uint)target;
        return (uint)instruction;
    }

    private static uint Invalid_Type(uint value, uint type, LocalLogger logger)
    {
        // TODO: Log error
        return value;
    }

}
