// Avishai Dernis 2026

namespace Zarem.N64.Devices.RealityCoProcessor.Enums;

/// <summary>
/// An enum for the fixed registers in the <see cref="N64VideoInterface"/>.
/// </summary>
public enum ViRegister
{
    /// <remarks>
    /// VI_STATUS_REG / VI_CONTROL_REG
    /// </remarks>
    Status = 0x00,

    /// <remarks>
    /// VI_ORIGIN_REG / VI_DRAM_ADDR_REG
    /// </remarks>
    Origin = 0x04,

    /// <remarks>
    /// VI_WIDTH_REG / VI_H_WIDTH_REG
    /// </remarks>
    Width = 0x08,

    /// <remarks>
    /// VI_V_INTR_REG
    /// </remarks>
    VerticalInterrupt = 0x0C,

    /// <remarks>
    /// VI_CURRENT_REG / VI_V_CURRENT_LINE_REG
    /// </remarks>
    Current = 0x10,

    /// <remarks>
    /// VI_BURST_REG
    /// </remarks>
    Burst = 0x14,

    /// <remarks>
    /// VI_V_SYNC_REG
    /// </remarks>
    VerticalSync = 0x18,

    /// <remarks>
    /// VI_H_SYNC_REG
    /// </remarks>
    HorizontalSync = 0x1C,

    /// <remarks>
    /// VI_LEAP_REG
    /// </remarks>
    Leap = 0x20,

    /// <remarks>
    /// VI_H_START_REG
    /// </remarks>
    HorizontalStart = 0x24,

    /// <remarks>
    /// VI_V_START_REG
    /// </remarks>
    VerticalStart = 0x28,

    /// <remarks>
    /// VI_V_BURST_REG
    /// </remarks>
    VerticalBurst = 0x2C,

    /// <remarks>
    /// VI_X_SCALE_REG
    /// </remarks>
    XScale = 0x30,

    /// <remarks>
    /// VI_Y_SCALE_REG
    /// </remarks>
    YScale = 0x34,
}
