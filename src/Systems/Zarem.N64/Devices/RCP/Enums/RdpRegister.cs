// Avishai Dernis 2026

namespace Zarem.N64.Devices.RCP.Enums;

/// <summary>
/// An enum for the fixed registers in the reality display processor.
/// </summary>
public enum RdpRegister
{
#pragma warning disable CS1591
    Start,
    End,
    Current,
    Status,
    Clock,
    BufferBusy,
    PipeBusy,
    TmemBusy,
#pragma warning restore CS1591
}
