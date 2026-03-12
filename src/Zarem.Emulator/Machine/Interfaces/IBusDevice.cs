// Avishai Dernis 2026

using System;

namespace Zarem.Emulator.Machine.Interfaces;

/// <summary>
/// An interface for a device hooked up to the memory bus.
/// </summary>
public interface IBusDevice
{
    /// <summary>
    /// Gets the name of the device.
    /// </summary>
    string Name { get; }

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
