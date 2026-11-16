// Avishai Dernis 2026

namespace Zarem.N64.Devices.RealityCoProcessor.Enums.Registers;

/// <summary>
/// An enum for the fixed registers in the <see cref="N64AudioInterface"/>.
/// </summary>
public enum SiRegister
{
    /// <summary>
    /// RDRAM address for Serial Interface DMA.
    /// </summary>
    /// <remarks>
    /// SI_DRAM_ADDR_REG
    /// </remarks>
    DramAddress = 0x00,

    /// <summary>
    /// Initiates 64-byte DMA read from PIF-RAM to RDRAM.
    /// </summary>
    /// <remarks>
    /// SI_PIF_ADDR_RD64B_REG
    /// </remarks>
    PifAddrRd64b = 0x04,

    /// <summary>
    /// Initiates 64-byte DMA write from RDRAM to PIF-RAM.
    /// </summary>
    /// <remarks>
    /// SI_PIF_ADDR_WR64B_REG
    /// </remarks>
    PifAddrWr64b = 0x10,

    /// <summary>
    /// Serial Interface status: DMA busy, IO busy, and interrupt flags.
    /// </summary>
    /// <remarks>
    /// SI_STATUS_REG
    /// </remarks>
    Status = 0x18,
}
