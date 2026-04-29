// Avishai Dernis 2026

using System;
using System.Buffers.Binary;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Machine.Devices.Interfaces;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.N64.Devices.Enums;

namespace Zarem.N64.Devices;

/// <summary>
/// An <see cref="IBusDevice"/> implementation for the N64 reality coprocessor (rcp) graphics card.
/// </summary>
public class RealityCoProcessor : IBusDevice
{
    private readonly PhysicalBus _bus;

    /// <summary>
    /// Initializes a new instance of the <see cref="RealityCoProcessor"/> class.
    /// </summary>
    public RealityCoProcessor(PhysicalBus bus)
    {
        _bus = bus;
    }

    /// <inheritdoc/>
    public string Name => "Reality CoProcessor";

    /// <inheritdoc/>
    public ulong BusRangeSize => Nintendo64.RcpSize;

    /// <inheritdoc/>
    public void Read(ulong offset, Span<byte> destination)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void Write(ulong offset, ReadOnlySpan<byte> source)
    {
        throw new NotImplementedException();
    }

    private void ExecuteGraphicsCommands(uint address)
    {
        // HLE LOGIC:
        // 1. Fetch the command from _memory at 'address'
        // 2. Decode the MIPS/RSP microcode instruction
        // 3. Instead of emulating the RSP (Reality Signal Processor) cycle-by-cycle,
        //    translate the command to a native API call (e.g., DrawTriangle, SetViewport)
    }

    /// <inheritdoc/>
    public void Dispose()
    {
    }
}
