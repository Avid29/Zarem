// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using Zarem.Emulator.Machine.Interfaces;

namespace Zarem.Emulator.Machine;

/// <summary>
/// A class for handling the mapping of memory and MMIO devices.
/// </summary>
public class MemoryMapper
{
    private readonly List<ulong> _sortedAddresses = [];
    private readonly Dictionary<ulong, IBusDevice> _devices = [];
    private bool _sorted = true;

    /// <summary>
    /// Map a new device onto the bus.
    /// </summary>
    /// <param name="baseAddress">The base address to register the device.</param>
    /// <param name="device">The device to register.</param>
    public void MapDevice(ulong baseAddress, IBusDevice device)
    {
        _sortedAddresses.Add(baseAddress);
        _devices.Add(baseAddress, device);
        _sorted = false;
    }

    /// <summary>
    /// Gets the device for a given a address.
    /// </summary>
    /// <param name="address">The address to resolve.</param>
    /// <param name="baseAddress">The base address the device is registered to.</param>
    /// <returns>The device registered to that address.</returns>
    public IBusDevice Resolve(ulong address, out ulong baseAddress)
    {
        if (!_sorted)
        {
            _sortedAddresses.Sort();
            _sorted = true;
        }

        int index = _sortedAddresses.BinarySearch(address);
        if (index < 0)
        {
            index = (~index) - 1;
        }

        baseAddress = _sortedAddresses[index];
        var device = _devices[baseAddress];

        if (address > baseAddress + device.BusRangeSize)
            throw new Exception($"No device mapped at address: 0x{address:X16}");

        return device;
    }
}
