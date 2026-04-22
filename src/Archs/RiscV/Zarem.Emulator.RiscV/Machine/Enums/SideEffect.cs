// Avishai Dernis 2026

using Zarem.Emulator.Interpret;

namespace Zarem.Emulator.Machine.Enums;

/// <summary>
/// An enum describing the secondary effect of an <see cref="RiscVExecution{T}"/>.
/// </summary>
public enum SideEffect
{
#pragma warning disable CS1591

    None,
    ReadMemory,
    ReadMemorySigned,
    WriteMemory,
    ProgramCounter,

#pragma warning restore CS1591
}
