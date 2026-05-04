// Avishai Dernis 2026

namespace Zarem.Emulator.Machine.Enums;

/// <summary>
/// An enum describing the priviledge mode the processor is operating in.
/// </summary>
public enum PrivilegeMode : byte
{

#pragma warning disable CS1591

    Kernel,
    Supervisor,
    User,

#pragma warning restore CS1591
}
