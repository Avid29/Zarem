// Avishai Dernis 2026

using Zarem.N64.Devices.RealityCoProcessor;

namespace Zarem.N64.Models.Enums.Registers;

/// <summary>
/// An enum for the fixed registers in the <see cref="N64MipsInterface"/>.
/// </summary>
public enum MiRegister
{
    /// <summary>
    /// MIPS Interface mode register.
    /// </summary>
    /// <remarks>
    /// MI_MODE_REG
    /// </remarks>
    Mode = 0x00,

    /// <summary>
    /// MIPS Interface version register.
    /// </summary>
    /// <remarks>
    /// MI_VERSION_REG
    /// </remarks>
    Version = 0x04,

    /// <summary>
    /// MIPS Interface interrupt register.
    /// </summary>
    /// <remarks>
    /// MI_INTR_REG
    /// </remarks>
    Interrupt = 0x08,

    /// <summary>
    /// MIPS Interface interrupt mask register.
    /// </summary>
    /// <remarks>
    /// MI_INTR_MASK_REG
    /// </remarks>
    InterruptMask = 0x0C,
}
