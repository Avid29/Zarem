// Avishai Dernis 2026

using Zarem.Attributes;

namespace Zarem.Z80.Models.Enums;

/// <summary>
/// An enum for Z80 reference and relocation types used during assembly and expression evaluation.
/// </summary>
public enum Z80ReferenceType : uint
{
    /// <summary>
    /// No relocation or reference modification. Literal value.
    /// </summary>
    None = 0,

    /// <summary>
    /// 16-bit absolute memory address or value (e.g., JP nn, CALL nn, LD HL, nn).
    /// Emitted as 2 bytes in Little-Endian format.
    /// </summary>
    [ReferenceType(16)]
    Absolute16 = 1,

    /// <summary>
    /// 8-bit absolute immediate value or I/O port address (e.g., LD A, n or IN A, (c)).
    /// </summary>
    [ReferenceType(8)]
    Absolute8 = 2,

    /// <summary>
    /// PC-relative 8-bit signed offset used by conditional and unconditional relative jumps (e.g., JR e, DJNZ e).
    /// Calculated as: TargetAddress - (CurrentInstructionAddress + 2).
    /// </summary>
    [ReferenceType("rel8", 8)]
    Relative8 = 3,

    /// <summary>
    /// High 8-bits of a 16-bit absolute expression or label (often supported by advanced z80 assemblers via a high() operator).
    /// </summary>
    [ReferenceType("hi", 8, ShiftAmount = 8)]
    High8 = 4,

    /// <summary>
    /// Low 8-bits of a 16-bit absolute expression or label (often supported via a low() operator).
    /// </summary>
    [ReferenceType("lo", 8)]
    Low8 = 5,

    /// <summary>
    /// Signed 8-bit index displacement offset used exclusively in index instructions (e.g., the 'd' in LD A, (IX+d)).
    /// </summary>
    [ReferenceType("idx_disp", 8)]
    IndexDisplacement = 6
}
