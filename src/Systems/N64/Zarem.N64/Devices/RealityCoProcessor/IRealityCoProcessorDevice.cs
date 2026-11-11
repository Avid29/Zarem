// Avishai Dernis 2026

using Zarem.Emulator.Devices.Interfaces;

namespace Zarem.N64.Devices.RealityCoProcessor;

/// <summary>
/// An interface for a <see cref="RealityCoProcessor"/> child device.
/// </summary>
public interface IRealityCoProcessorDevice : IBusDevice
{
    /// <summary>
    /// Gets the base address of the device in the RCP bus mapping.
    /// </summary>
    ulong BusRangeBase { get; }
}
