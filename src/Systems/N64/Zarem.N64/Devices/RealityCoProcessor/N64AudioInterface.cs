// Avishai Dernis 2026

using System;
using Zarem.Emulator.Devices.MemoryBacking;

namespace Zarem.N64.Devices.RealityCoProcessor;

/// <summary>
/// A sub-components of the <see cref="RealityCoProcessor"/> responsible for ...
/// </summary>
public class N64AudioInterface : IRealityCoProcessorDevice
{
    private readonly FlatMemoryStore _regs;

    /// <summary>
    /// Initializes a new instance of the <see cref="N64AudioInterface"/> class.
    /// </summary>
    public N64AudioInterface()
    {
        _regs = new FlatMemoryStore(BusRangeSize);
    }

    /// <inheritdoc/>
    public string Name => "N64 Audio Interface (AI)";

    /// <inheritdoc/>
    public ulong BusRangeBase => 0x0450_0000;

    /// <inheritdoc/>
    public ulong BusRangeSize => 0x19;

    /// <inheritdoc/>
    public void Read(ulong offset, Span<byte> destination) => _regs.Read(offset, destination);

    /// <inheritdoc/>
    public void Write(ulong offset, ReadOnlySpan<byte> source)
    {
        // TODO: Handle side effects

        _regs.Write(offset, source);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _regs.Dispose();
    }
}
