// Avishai Dernis 2026

namespace Zarem.N64.Devices.RealityCoProcessor.Enums;

/// <summary>
/// An enum for the fixed registers in the <see cref="N64AudioInterface"/>.
/// </summary>
public enum AiRegister
{
    /// <remarks>
    /// AI_DRAM_ADDR_REG (RDRAM source address for audio samples)
    /// </remarks>
    DramAddress = 0x00,

    /// <remarks>
    /// AI_LEN_REG (Transfer length in bytes, triggers DMA)
    /// </remarks>
    Length = 0x04,

    /// <remarks>
    /// AI_CONTROL_REG (Audio DMA enable / disable)
    /// </remarks>
    Control = 0x08,

    /// <remarks>
    /// AI_STATUS_REG (FIFO full status and interrupt clear)
    /// </remarks>
    Status = 0x0C,

    /// <remarks>
    /// AI_DACRATE_REG (DAC sample rate generator)
    /// </remarks>
    DacRate = 0x10,

    /// <remarks>
    /// AI_BITRATE_REG (Audio bit rate configuration)
    /// </remarks>
    BitRate = 0x14,

    /// <remarks>
    /// AI_STATUS_REG (Alternate alias / sample counter depending on microcode/version)
    /// </remarks>
    StatusAlternate = 0x18,
}
