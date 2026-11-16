// Avishai Dernis 2026

namespace Zarem.N64.Devices.RealityCoProcessor.Enums.Registers;

/// <summary>
/// An enum for the fixed registers in the reality display processor.
/// </summary>
public enum RdpRegister
{
    /// <summary>
    /// DP command buffer start address.
    /// </summary>
    /// <remarks>
    /// DPC_START_REG
    /// </remarks>
    Start = 0x00,

    /// <summary>
    /// DP command buffer end address.
    /// </summary>
    /// <remarks>
    /// DPC_END_REG
    /// </remarks>
    End = 0x04,

    /// <summary>
    /// DP command buffer current address.
    /// </summary>
    /// <remarks>
    /// DPC_CURRENT_REG
    /// </remarks>
    Current = 0x08,

    /// <summary>
    /// Display Processor status register.
    /// </summary>
    /// <remarks>
    /// DPC_STATUS_REG
    /// </remarks>
    Status = 0x0C,

    /// <summary>
    /// RDP clock counter.
    /// </summary>
    /// <remarks>
    /// DPC_CLOCK_REG
    /// </remarks>
    Clock = 0x10,

    /// <summary>
    /// RDP command buffer busy cycle counter.
    /// </summary>
    /// <remarks>
    /// DPC_BUFBUSY_REG
    /// </remarks>
    BufferBusy = 0x14,

    /// <summary>
    /// RDP pipeline busy cycle counter.
    /// </summary>
    /// <remarks>
    /// DPC_PIPEBUSY_REG
    /// </remarks>
    PipeBusy = 0x18,

    /// <summary>
    /// RDP TMEM busy cycle counter.
    /// </summary>
    /// <remarks>
    /// DPC_TMEMBUSY_REG
    /// </remarks>
    TmemBusy = 0x1C,
}
