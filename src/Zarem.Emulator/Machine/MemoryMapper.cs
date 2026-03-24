// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using Zarem.Emulator.Machine.Devices.Interfaces;

namespace Zarem.Emulator.Machine;

/// <summary>
/// A class for handling the mapping of memory and MMIO devices.
/// </summary>
public class MemoryMapper
{
    private readonly record struct DeviceMapping(ulong BaseAddress, IBusDevice? Device);

    private readonly DeviceMapping[] _pageTable = new DeviceMapping[1024 * 1024];
    private readonly List<IBusDevice> _devices = [];

    /// <summary>
    /// Gets an <see cref="IEnumerable{IDevice}"/> of the registered devices.
    /// </summary>
    public IEnumerable<IDevice> Devices => _devices;

    /// <summary>
    /// Map a new device onto the bus.
    /// </summary>
    /// <param name="baseAddress">The base address to register the device.</param>
    /// <param name="device">The device to register.</param>
    public void MapDevice(ulong baseAddress, IBusDevice device)
    {
        uint startPage = (uint)(baseAddress >> 12);
        uint pageCount = (uint)(device.BusRangeSize >> 12);

        var mapping = new DeviceMapping(baseAddress, device);
        for (uint i = 0; i < pageCount; i++)
        {
            _pageTable[startPage + i] = mapping;
        }

        _devices.Add(device);
    }

    /// <summary>
    /// Gets the device for a given a address.
    /// </summary>
    /// <param name="address">The address to resolve.</param>
    /// <param name="baseAddress">The base address the device is registered to.</param>
    /// <returns>The device registered to that address.</returns>
    public IBusDevice Resolve(ulong address, out ulong baseAddress)
    {
        var mapping = _pageTable[address >> 12];
        if (mapping.Device is null)
        {
            throw new Exception($"No device mapped at address: 0x{address:X16}");
        }

        baseAddress = mapping.BaseAddress;
        return mapping.Device;
    }
}
