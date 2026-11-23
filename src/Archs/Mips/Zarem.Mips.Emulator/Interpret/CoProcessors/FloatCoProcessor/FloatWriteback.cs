// Avishai Dernis 2026

namespace Zarem.Mips.Emulator.Interpret.CoProcessors.FloatCoProcessor;

/// <summary>
/// An enum for the float writeback format.
/// </summary>
public enum FloatWriteback
{
#pragma warning disable CS1591
    Word,
    Long,
    Control,
#pragma warning restore CS1591
}
