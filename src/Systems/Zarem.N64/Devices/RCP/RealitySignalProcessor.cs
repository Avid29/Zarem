// Avishai Dernis 2026

using System;
using Zarem.Emulator.Machine;

namespace Zarem.N64.Devices.RCP;

/// <summary>
/// A sub-components of the <see cref="RealityCoProcessor"/> responsible for processing the DMA transfers to/from the RCP.
/// </summary>
public class RealitySignalProcessor
{
    private readonly RealityCoProcessor _rcp;
    private readonly PhysicalBus _bus;

    /// <summary>
    /// Initializes a new instance of the <see cref="RealitySignalProcessor"/> class.
    /// </summary>
    public RealitySignalProcessor(RealityCoProcessor rcp, PhysicalBus bus)
    {
        _rcp = rcp;
        _bus = bus;
    }

    /// <summary>
    /// Writes the specified data to memory at the given offset.
    /// </summary>
    public void WriteMemory(ulong offset, ReadOnlySpan<byte> data)
    {

    }

    /// <summary>
    /// Writes the specified data at the given offset to the RSP registers.
    /// </summary>
    public void WriteRegister(ulong offset, ReadOnlySpan<byte> data)
    {

    }
}
