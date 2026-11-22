// Avishai Dernis 2026

namespace Zarem.N64.Devices.VideoInterface;

/// <summary>
/// An enum for the fixed registers in the <see cref="N64VideoInterface"/>.
/// </summary>
public enum ViRegister
{
    /// <summary>
    /// Video interface status and control register.
    /// </summary>
    /// <remarks>
    /// VI_STATUS_REG / VI_CONTROL_REG
    /// </remarks>
    Status = 0x00,

    /// <summary>
    /// Video interface frame buffer origin (RDRAM address) register.
    /// </summary>
    /// <remarks>
    /// VI_ORIGIN_REG / VI_DRAM_ADDR_REG
    /// </remarks>
    Origin = 0x04,

    /// <summary>
    /// Video interface screen width register.
    /// </summary>
    /// <remarks>
    /// VI_WIDTH_REG / VI_H_WIDTH_REG
    /// </remarks>
    Width = 0x08,

    /// <summary>
    /// Video interface vertical interrupt line register.
    /// </summary>
    /// <remarks>
    /// VI_V_INTR_REG
    /// </remarks>
    VerticalInterrupt = 0x0C,

    /// <summary>
    /// Video interface current halfline counter register.
    /// </summary>
    /// <remarks>
    /// VI_CURRENT_REG / VI_V_CURRENT_LINE_REG
    /// </remarks>
    Current = 0x10,

    /// <summary>
    /// Video interface horizontal video burst register.
    /// </summary>
    /// <remarks>
    /// VI_BURST_REG
    /// </remarks>
    Burst = 0x14,

    /// <summary>
    /// Video interface vertical sync width register.
    /// </summary>
    /// <remarks>
    /// VI_V_SYNC_REG
    /// </remarks>
    VerticalSync = 0x18,

    /// <summary>
    /// Video interface horizontal sync width register.
    /// </summary>
    /// <remarks>
    /// VI_H_SYNC_REG
    /// </remarks>
    HorizontalSync = 0x1C,

    /// <summary>
    /// Video interface leap pattern register for interlaced video.
    /// </summary>
    /// <remarks>
    /// VI_LEAP_REG
    /// </remarks>
    Leap = 0x20,

    /// <summary>
    /// Video interface horizontal active video start and stop register.
    /// </summary>
    /// <remarks>
    /// VI_H_START_REG
    /// </remarks>
    HorizontalStart = 0x24,

    /// <summary>
    /// Video interface vertical active video start and stop register.
    /// </summary>
    /// <remarks>
    /// VI_V_START_REG
    /// </remarks>
    VerticalStart = 0x28,

    /// <summary>
    /// Video interface vertical video burst start and stop register.
    /// </summary>
    /// <remarks>
    /// VI_V_BURST_REG
    /// </remarks>
    VerticalBurst = 0x2C,

    /// <summary>
    /// Video interface horizontal scaling register.
    /// </summary>
    /// <remarks>
    /// VI_X_SCALE_REG
    /// </remarks>
    XScale = 0x30,

    /// <summary>
    /// Video interface vertical scaling register.
    /// </summary>
    /// <remarks>
    /// VI_Y_SCALE_REG
    /// </remarks>
    YScale = 0x34,
}
