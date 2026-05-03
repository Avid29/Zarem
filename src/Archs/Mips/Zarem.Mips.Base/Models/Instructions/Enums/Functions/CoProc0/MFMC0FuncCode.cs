// Avishai Dernis 2025

namespace Zarem.Models.Instructions.Enums.Functions.CoProc0;

/// <summary>
/// An enum for <see cref="CoProc0RSCode.MFMC0"/> instruction function codes.
/// </summary>
public enum MFMC0FuncCode
{
    #pragma warning disable CS1591

    DisableInterrupts = 0x0,
    DisableVirtualProcessor = 0x4,
    EnableInterrupts = 0x20,
    EnableVirtualProcessor = 0x24,

    #pragma warning restore CS1591
}
