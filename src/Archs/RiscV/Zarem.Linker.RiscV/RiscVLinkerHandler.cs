// Avishai Dernis 2026

using Zarem.Assembler.Logging;
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
/// A linker handler for the RISC-V architecture.
/// </summary>
public class RiscVLinkerHandler : ILinkerHandler<RiscVLinkerConfig>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVLinkerHandler"/> class.
    /// </summary>
    public RiscVLinkerHandler(RiscVLinkerConfig config)
    {
        Config = config;
    }

    /// <inheritdoc/>
    public string GetArchitectureName() => "RISC-V";

    /// <inheritdoc/>
    public RiscVLinkerConfig Config { get; }

    /// <inheritdoc/>
    public bool PatchRelocation(Section section, RelocationEntry relocation, ulong offset, ulong symbolVirtual, ulong patchVirtual, ILogger logger)
    {
        var localLogger = new LinkerLogger(logger);

        section.Position = (long)offset;
        section.Stream.TryRead<uint>(out var value, true);

        var instruction = (RiscVInstruction)value;

        long target = (long)symbolVirtual + relocation.Addend;
        long relativeTarget = target - ((long)patchVirtual + 4);

        value = (RiscVReferenceType)relocation.Type switch
        {
            RiscVReferenceType.Low12 => RISCV_Low12(instruction, target),
            RiscVReferenceType.High20 => RISCV_High20(instruction, target),
            RiscVReferenceType.Jump20 => RISCV_Jump20(instruction, target),
            _ => Invalid_Type(value, relocation.Type, localLogger)
        };

        section.Position -= sizeof(uint);
        section.Stream.TryWrite(value, true);
        return true;
    }

    private static uint RISCV_Low12(RiscVInstruction instruction, long target)
    {
        instruction.Immediate = (short)(target & 0x0FFF);
        return (uint)instruction;
    }

    private static uint RISCV_High20(RiscVInstruction instruction, long target)
    {
        instruction.UpperImmediate = (int)((uint)(target + 0x800) >> 12);
        return (uint)instruction;
    }

    private static uint RISCV_Jump20(RiscVInstruction instruction, long relativeTarget)
    {
        instruction.JumpOffset = (int)relativeTarget;
        return (uint)instruction;
    }

    private static uint Invalid_Type(uint value, uint type, LocalLogger logger)
    {
        // TODO: Log error
        return value;
    }
}
