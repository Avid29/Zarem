// Avishai Dernis 2026

using System;

namespace Zarem.N64.Devices.RealityCoProcessor;

/// <summary>
/// A sub-components of the <see cref="RealityCoProcessor"/> responsible for ...
/// </summary>
public class RealitySignalProcessor : IRealityCoProcessorDevice
{
    /// <inheritdoc/>
    public string Name => "Reality Signal Processor (RSP)";

    /// <inheritdoc/>
    public ulong BusRangeBase => 0x0400_0000;

    /// <inheritdoc/>
    public ulong BusRangeSize => 0x4_0020;

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

    /// <inheritdoc/>
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
