// Avishai Dernis 2026

using Zarem.Assembler.Logging;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Extensions.System.IO;
using Zarem.Linker.Config;
using Zarem.Linker.Handlers;
using Zarem.Linker.Logging;
using Zarem.Models.Instructions;
using Zarem.Models.Tables;
using Zarem.Models.Tables.Enums;

namespace Zarem.Linker.Handler;

/// <summary>
/// A linker handler for the MIPS architecture.
/// </summary>
public class MIPSLinkerHandler : ILinkerHandler<MIPSLinkerConfig>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MIPSLinkerHandler"/> class.
    /// </summary>
    public MIPSLinkerHandler(MIPSLinkerConfig config)
    {
        Config = config;
    }

    /// <inheritdoc/>
    public string GetArchitectureName() => "MIPS";

    /// <inheritdoc/>
    public MIPSLinkerConfig Config { get; }

    /// <inheritdoc/>
    public bool PatchRelocation(Section section, RelocationEntry relocation, ulong offset, ulong symbolVirtual, ulong patchVirtual, ILogger logger)
    {
        var localLogger = new LinkerLogger(logger);

        section.Position = (long)offset;
        section.Stream.TryRead<uint>(out var value);

        var instruction = (MIPSInstruction)value;

        long target = (long)symbolVirtual + relocation.Addend;
        long pcTarget = target - ((long)patchVirtual + 4);

        value = (MipsReferenceType)relocation.Type switch
        {
            MipsReferenceType.Low16 => MIPS_Low16(instruction, target),
            MipsReferenceType.High16 => MIPS_High16(instruction, target),
            MipsReferenceType.PCRelative16 => MIPS_PC16(instruction, pcTarget),
            MipsReferenceType.JumpTarget26 => MIPS_Jump26(instruction, target, patchVirtual, relocation, localLogger),
            MipsReferenceType.Absolute32 => (uint)target,
            _ => Invalid_Type(value, relocation.Type, localLogger)
        };

        section.Position -= sizeof(uint);
        section.Stream.TryWrite(value);
        return true;
    }

    private static uint MIPS_Low16(MIPSInstruction instruction, long target)
    {
        instruction.ImmediateValue = (short)(target & 0xFFFF);
        return (uint)instruction;
    }

    private static uint MIPS_High16(MIPSInstruction instruction, long target)
    {
        instruction.ImmediateValue = (short)((target + 0x8000) >> 16);
        return (uint)instruction;
    }

    private static uint MIPS_PC16(MIPSInstruction instruction, long pcTarget)
    {
        // TODO: Log error, branch out of range

        instruction.Offset = (int)pcTarget;
        return (uint)instruction;
    }

    private static uint MIPS_Jump26(MIPSInstruction instruction, long target, ulong patchVirtual, RelocationEntry relocation, LinkerLogger logger)
    {
        if (((ulong)target & 0xF0000000UL) != (patchVirtual & 0xF0000000UL))
        {
            // TODO: Log error, jump out of range.
            logger.Log(Severity.Error, LogId.JumpOutOfRange, "TODO: Get file name", "JumpOutOfRange", relocation.SymbolName);
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
