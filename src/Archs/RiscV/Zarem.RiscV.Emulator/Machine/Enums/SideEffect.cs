// Avishai Dernis 2026


// Avishai Dernis 2026

using Zarem.RiscV.Emulator.Interpret;

namespace Zarem.RiscV.Emulator.Machine.Enums;

/// <summary>
/// An enum describing the secondary effect of an <see cref="RiscVExecution{T}"/>.
/// </summary>
public enum RiscVSideEffect
{
#pragma warning disable CS1591

    None,
    ProgramCounter,
    ReadMemory,
    ReadMemorySigned,
    WriteMemory,
    WriteHalf,
    WriteSingle,
    WriteDouble,
    WriteQuad,

#pragma warning restore CS1591
}
