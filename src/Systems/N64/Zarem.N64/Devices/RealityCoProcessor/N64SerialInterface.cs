// Avishai Dernis 2026

using System;

namespace Zarem.N64.Devices.RealityCoProcessor;

/// <summary>
/// A sub-components of the <see cref="RealityCoProcessor"/> responsible for ...
/// </summary>
public class N64SerialInterface : IRealityCoProcessorDevice
{
    /// <inheritdoc/>
    public string Name => "N64 Serial Interface (SI)";

    /// <inheritdoc/>
    public ulong BusRangeBase => 0x0480_0000;

    /// <inheritdoc/>
    public ulong BusRangeSize => 0x1D;

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
