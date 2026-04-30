// Avishai Dernis 2026

using System;

namespace Zarem.Emulator.Devices.Interfaces;

/// <summary>
/// An interface for an <see cref="IDevice"/> hooked up to the memory bus.
/// </summary>
public interface IBusDevice : IDevice
{
    /// <summary>
    /// Gets the size of the range the device uses on the bus.
    /// </summary>
    ulong BusRangeSize { get; }

    /// <summary>
    /// Reads data from the device into the provided span.
    /// </summary>
    void Read(ulong offset, Span<byte> destination);

    /// <summary>
    /// Writes data from the span into the device.
    /// </summary>
    void Write(ulong offset, ReadOnlySpan<byte> source);
}
