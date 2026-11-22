// Avishai Dernis 2026

namespace Zarem.N64.Devices.AudioInterface;

/// <summary>
/// An enum for the fixed registers in the <see cref="N64AudioInterface"/>.
/// </summary>
public enum AiRegister
{
    /// <summary>
    /// RDRAM source address for audio samples.
    /// </summary>
    /// <remarks>
    /// AI_DRAM_ADDR_REG
    /// </remarks>
    DramAddress = 0x00,

    /// <summary>
    /// Transfer length in bytes, triggers DMA.
    /// </summary>
    /// <remarks>
    /// AI_LEN_REG
    /// </remarks>
    Length = 0x04,

    /// <summary>
    /// Audio DMA enable / disable.
    /// </summary>
    /// <remarks>
    /// AI_CONTROL_REG
    /// </remarks>
    Control = 0x08,

    /// <summary>
    /// FIFO full status and interrupt clear.
    /// </summary>
    /// <remarks>
    /// AI_STATUS_REG
    /// </remarks>
    Status = 0x0C,

    /// <summary>
    /// DAC sample rate generator.
    /// </summary>
    /// <remarks>
    /// AI_DACRATE_REG
    /// </remarks>
    DacRate = 0x10,

    /// <summary>
    /// Audio bit rate configuration.
    /// </summary>
    /// <remarks>
    /// AI_BITRATE_REG
    /// </remarks>
    BitRate = 0x14,

    /// <summary>
    /// Alternate alias / sample counter depending on microcode/version.
    /// </summary>
    /// <remarks>
    /// AI_STATUS_REG
    /// </remarks>
    StatusAlternate = 0x18,
}
