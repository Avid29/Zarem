// Avishai Dernis 2026

using System;
using Zarem.Emulator.Devices.MemoryBacking;
using Zarem.N64.Devices.RealityCoProcessor.Abstract;

namespace Zarem.N64.Devices.RealityCoProcessor;

/// <summary>
/// A sub-components of the <see cref="RealityCoProcessor"/> responsible for ...
/// </summary>
public class N64RamInterface : IRealityCoProcessorDevice
{
    private readonly FlatMemoryStore _regs;

    /// <summary>
    /// Initializes a new instance of the <see cref="N64RamInterface"/> class.
    /// </summary>
    public N64RamInterface()
    {
        _regs = new FlatMemoryStore(BusRangeBase);

        // Initialize standard default configuration values for RDRAM/RI registers if required
        // e.g., default latency or mode settings
    }

    /// <inheritdoc/>
    public string Name => "N64 RAM Interface (RI)";

    /// <inheritdoc/>
    public ulong BusRangeBase => 0x0470_0000;

    /// <inheritdoc/>
    public ulong BusRangeSize => 0x20;

    /// <inheritdoc/>
    public void Read(ulong offset, Span<byte> destination) => _regs.Read(offset, destination);

    /// <inheritdoc/>
    public void Write(ulong offset, ReadOnlySpan<byte> source) => _regs.Write(offset, source);

    /// <inheritdoc/>
    public void Dispose() => _regs.Dispose();
}
