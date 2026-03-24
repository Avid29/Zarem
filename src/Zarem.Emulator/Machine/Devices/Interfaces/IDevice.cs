// Avishai Dernis 2026

using System;

namespace Zarem.Emulator.Machine.Devices.Interfaces;

/// <summary>
/// An interface for a device in a computer.
/// </summary>
public interface IDevice : IDisposable
{
    /// <summary>
    /// Gets the name of the device.
    /// </summary>
    string Name { get; }
}
