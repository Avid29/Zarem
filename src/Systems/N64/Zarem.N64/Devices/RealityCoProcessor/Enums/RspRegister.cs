// Avishai Dernis 2026

namespace Zarem.N64.Devices.RealityCoProcessor.Enums;

/// <summary>
/// An enum for the fixed registers in the reality signal processor.
/// </summary>
public enum RspRegister
{
    /// <remarks>
    /// SP_MEM_ADDR_REG
    /// </remarks>
    MemoryAddress = 0x00,

    /// <remarks>
    /// SP_DRAM_ADDR_REG
    /// </remarks>
    DramAddress = 0x04,

    /// <remarks>
    /// SP_RD_LEN_REG
    /// </remarks>
    ReadLength = 0x8,

    /// <remarks>
    /// SP_WR_LEN_REG
    /// </remarks>
    WriteLength = 0xC,

    /// <remarks>
    /// SP_STATUS_REG
    /// </remarks>
    Status = 0x10,

    /// <remarks>
    /// SP_DMA_FULL_REG
    /// </remarks>
    DmaFull = 0x14,

    /// <remarks>
    /// SP_DMA_BUSY_REG
    /// </remarks>
    DmaBusy = 0x18,

    /// <remarks>
    /// SP_SEMAPHORE_REG
    /// </remarks>
    Semaphore = 0x1C,
}
