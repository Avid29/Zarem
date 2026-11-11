// Avishai Dernis 2026

using System;

namespace Zarem.N64.Devices.RealityCoProcessor;

/// <summary>
/// A sub-components of the <see cref="RealityCoProcessor"/> responsible for output and timing.
/// </summary>
public class N64VideoInterface : IRealityCoProcessorDevice
{
    /// <inheritdoc/>
    public string Name => "N64 Video Interface (VI)";

    /// <inheritdoc/>
    public ulong BusRangeBase => 0x0440_0000;

    /// <inheritdoc/>
    public ulong BusRangeSize => 0x39;

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
