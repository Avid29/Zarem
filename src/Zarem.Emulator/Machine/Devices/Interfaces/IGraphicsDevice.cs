// Avishai Dernis 2026

using CommunityToolkit.HighPerformance;
using System;

namespace Zarem.Emulator.Machine.Devices.Interfaces;

/// <summary>
/// An <see cref="IBusDevice"/> for a graphics device.
/// </summary>
public interface IGraphicsDevice : IDevice
{
    /// <summary>
    /// An event invoked when the graphics device signals a refresh.
    /// </summary>
    event EventHandler? Refresh;

    /// <summary>
    /// Gets the pixel buffer for graphics device.
    /// </summary>
    ReadOnlySpan2D<uint> GetPixelBuffer();
}
