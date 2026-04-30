// Avishai Dernis 2026


// Avishai Dernis 2026

namespace Zarem.Emulator.Devices.Interfaces;

/// <summary>
/// An interface for a <see cref="IBusDevice"/> which can be read/written to directly.
/// </summary>
public unsafe interface IBusDeviceDirect : IBusDevice
{
    /// <summary>
    /// Gets a pointer to an address within the ram device.
    /// </summary>
    /// <param name="offset">The offset within the device address range.</param>
    /// <returns>A pointer to the requested address.</returns>
    byte* GetPointer(ulong offset);
}
