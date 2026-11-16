// Avishai Dernis 2026

namespace Zarem.N64.Devices.RealityCoProcessor.Enums;

/// <summary>
/// An enum for the fixed registers in the <see cref="N64ParallelInterface"/>.
/// </summary>
public enum PiRegister
{
    /// <remarks>
    /// PI_DRAM_ADDR_REG
    /// </remarks>
    DramAddress = 0x00,

    /// <remarks>
    /// PI_CART_ADDR_REG
    /// </remarks>
    CartridgeAddress = 0x04,

    /// <remarks>
    /// PI_RD_LEN_REG
    /// </remarks>
    ReadLength = 0x08,

    /// <remarks>
    /// PI_WR_LEN_REG
    /// </remarks>
    WriteLength = 0x0C,

    /// <remarks>
    /// PI_STATUS_REG
    /// </remarks>
    Status = 0x10,

    /// <remarks>
    /// PI_BSD_DOM1_LAT_REG
    /// </remarks>
    BsdDomain1Latency = 0x14,

    /// <remarks>
    /// PI_BSD_DOM1_PWD_REG
    /// </remarks>
    BsdDomain1PulseWidth = 0x18,

    /// <remarks>
    /// PI_BSD_DOM1_PGS_REG
    /// </remarks>
    BsdDomain1PageSize = 0x1C,

    /// <remarks>
    /// PI_BSD_DOM1_RLS_REG
    /// </remarks>
    BsdDomain1Release = 0x20,

    /// <remarks>
    /// PI_BSD_DOM2_LAT_REG
    /// </remarks>
    BsdDomain2Latency = 0x24,

    /// <remarks>
    /// PI_BSD_DOM2_PWD_REG
    /// </remarks>
    BsdDomain2PulseWidth = 0x28,

    /// <remarks>
    /// PI_BSD_DOM2_PGS_REG
    /// </remarks>
    BsdDomain2PageSize = 0x2C,

    /// <remarks>
    /// PI_BSD_DOM2_RLS_REG
    /// </remarks>
    BsdDomain2Release = 0x30,
}
