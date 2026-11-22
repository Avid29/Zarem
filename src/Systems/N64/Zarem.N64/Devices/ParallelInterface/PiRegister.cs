// Avishai Dernis 2026

namespace Zarem.N64.Devices.ParallelInterface;

/// <summary>
/// An enum for the fixed registers in the <see cref="N64ParallelInterface"/>.
/// </summary>
public enum PiRegister
{
    /// <summary>
    /// RDRAM address for Peripheral Interface DMA.
    /// </summary>
    /// <remarks>
    /// PI_DRAM_ADDR_REG
    /// </remarks>
    DramAddress = 0x00,

    /// <summary>
    /// Cartridge address for Peripheral Interface DMA.
    /// </summary>
    /// <remarks>
    /// PI_CART_ADDR_REG
    /// </remarks>
    CartridgeAddress = 0x04,

    /// <summary>
    /// Transfer length for reads from cartridge to RDRAM, triggers DMA.
    /// </summary>
    /// <remarks>
    /// PI_RD_LEN_REG
    /// </remarks>
    ReadLength = 0x08,

    /// <summary>
    /// Transfer length for writes from RDRAM to cartridge, triggers DMA.
    /// </summary>
    /// <remarks>
    /// PI_WR_LEN_REG
    /// </remarks>
    WriteLength = 0x0C,

    /// <summary>
    /// Peripheral Interface status: DMA busy, IO busy, and error flags.
    /// </summary>
    /// <remarks>
    /// PI_STATUS_REG
    /// </remarks>
    Status = 0x10,

    /// <summary>
    /// BSD Domain 1 latency configuration.
    /// </summary>
    /// <remarks>
    /// PI_BSD_DOM1_LAT_REG
    /// </remarks>
    BsdDomain1Latency = 0x14,

    /// <summary>
    /// BSD Domain 1 pulse width configuration.
    /// </summary>
    /// <remarks>
    /// PI_BSD_DOM1_PWD_REG
    /// </remarks>
    BsdDomain1PulseWidth = 0x18,

    /// <summary>
    /// BSD Domain 1 page size configuration.
    /// </summary>
    /// <remarks>
    /// PI_BSD_DOM1_PGS_REG
    /// </remarks>
    BsdDomain1PageSize = 0x1C,

    /// <summary>
    /// BSD Domain 1 release configuration.
    /// </summary>
    /// <remarks>
    /// PI_BSD_DOM1_RLS_REG
    /// </remarks>
    BsdDomain1Release = 0x20,

    /// <summary>
    /// BSD Domain 2 latency configuration.
    /// </summary>
    /// <remarks>
    /// PI_BSD_DOM2_LAT_REG
    /// </remarks>
    BsdDomain2Latency = 0x24,

    /// <summary>
    /// BSD Domain 2 pulse width configuration.
    /// </summary>
    /// <remarks>
    /// PI_BSD_DOM2_PWD_REG
    /// </remarks>
    BsdDomain2PulseWidth = 0x28,

    /// <summary>
    /// BSD Domain 2 page size configuration.
    /// </summary>
    /// <remarks>
    /// PI_BSD_DOM2_PGS_REG
    /// </remarks>
    BsdDomain2PageSize = 0x2C,

    /// <summary>
    /// BSD Domain 2 release configuration.
    /// </summary>
    /// <remarks>
    /// PI_BSD_DOM2_RLS_REG
    /// </remarks>
    BsdDomain2Release = 0x30,
}
