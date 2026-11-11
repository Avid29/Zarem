// Avishai Dernis 2026

using System;

namespace Zarem.N64.Devices.RealityCoProcessor;

/// <summary>
/// A sub-components of the <see cref="RealityCoProcessor"/> responsible for ...
/// </summary>
public class N64ParallelInterface : IRealityCoProcessorDevice
{
    /// <inheritdoc/>
    public string Name => "N64 Parallel Interface (PI)";

    /// <inheritdoc/>
    public ulong BusRangeBase => 0x0460_0000;

    /// <inheritdoc/>
    public ulong BusRangeSize => 0x3D;

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
