// Avishai Dernis 2026

namespace Zarem.N64.Devices.RealitySignalProcessor;

/// <summary>
/// An enum for the fixed registers in the reality signal processor.
/// </summary>
public enum RspRegister
{
    /// <summary>
    /// RSP memory (IMEM/DMEM) address for DMA.
    /// </summary>
    /// <remarks>
    /// SP_MEM_ADDR_REG
    /// </remarks>
    MemoryAddress = 0x00,

    /// <summary>
    /// RDRAM address for RSP DMA.
    /// </summary>
    /// <remarks>
    /// SP_DRAM_ADDR_REG
    /// </remarks>
    DramAddress = 0x04,

    /// <summary>
    /// Transfer length for reads from RDRAM to RSP memory, triggers DMA.
    /// </summary>
    /// <remarks>
    /// SP_RD_LEN_REG
    /// </remarks>
    ReadLength = 0x08,

    /// <summary>
    /// Transfer length for writes from RSP memory to RDRAM, triggers DMA.
    /// </summary>
    /// <remarks>
    /// SP_WR_LEN_REG
    /// </remarks>
    WriteLength = 0x0C,

    /// <summary>
    /// RSP status and control flags.
    /// </summary>
    /// <remarks>
    /// SP_STATUS_REG
    /// </remarks>
    Status = 0x10,

    /// <summary>
    /// RSP DMA full status flag.
    /// </summary>
    /// <remarks>
    /// SP_DMA_FULL_REG
    /// </remarks>
    DmaFull = 0x14,

    /// <summary>
    /// RSP DMA busy status flag.
    /// </summary>
    /// <remarks>
    /// SP_DMA_BUSY_REG
    /// </remarks>
    DmaBusy = 0x18,

    /// <summary>
    /// Inter-processor semaphore flag.
    /// </summary>
    /// <remarks>
    /// SP_SEMAPHORE_REG
    /// </remarks>
    Semaphore = 0x1C,
}
