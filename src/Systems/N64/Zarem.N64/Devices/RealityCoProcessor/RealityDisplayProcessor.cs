// Avishai Dernis 2026

using System;

namespace Zarem.N64.Devices.RealityCoProcessor;

#pragma warning disable CS0649

/// <summary>
/// A sub-components of the <see cref="RealityCoProcessor"/> responsible for processing the display operations.
/// </summary>
public partial class RealityDisplayProcessor : IRealityCoProcessorDevice
{
    /// <inheritdoc/>
    public string Name => "Reality Display Processor (RDP)";

    /// <inheritdoc/>
    public ulong BusRangeBase => 0x0410_0000;

    /// <inheritdoc/>
    public ulong BusRangeSize => 0x10_0010;

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
