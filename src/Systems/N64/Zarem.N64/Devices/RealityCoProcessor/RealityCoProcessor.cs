// Avishai Dernis 2026

using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using Zarem.Emulator.Devices.Interfaces;
using Zarem.Emulator.Machine.Memory;

namespace Zarem.N64.Devices.RealityCoProcessor;

/// <summary>
/// An <see cref="IBusDevice"/> implementation for the N64 reality coprocessor (rcp) graphics card.
/// </summary>
public class RealityCoProcessor : IBusDevice, IDXGraphicsDevice
{
#pragma warning disable CS1591
    public const ulong MappingBase = 0x0440_0000;
    public const ulong MappingSize = 0x39;
#pragma warning restore CS1591

    private readonly IRealityCoProcessorDevice[] _childDevices;

    /// <summary>
    /// Initializes a new instance of the <see cref="RealityCoProcessor"/> class.
    /// </summary>
    public RealityCoProcessor(PhysicalBus bus)
    {
        _childDevices =
        [
            new RealitySignalProcessor(),
            new RealityDisplayProcessor(),
            new N64MipsInterface(),
            new N64VideoInterface(),
            new N64AudioInterface(),
            new N64ParallelInterface(),
            new N64RamInterface(),
            new N64SerialInterface(),
        ];
    }

    /// <inheritdoc/>
    public string Name => "Reality CoProcessor (RCP)";

    /// <inheritdoc/>
    public ulong BusRangeSize => MappingSize;

    /// <inheritdoc/>
    public void Read(ulong offset, Span<byte> destination)
    {
        var childDevice = GetChildDevice(offset, out var deviceOffset);
        childDevice.Read(deviceOffset, destination);
    }

    /// <inheritdoc/>
    public void Write(ulong offset, ReadOnlySpan<byte> source)
    {
        var childDevice = GetChildDevice(offset, out var deviceOffset);
        childDevice.Write(deviceOffset, source);
    }

    /// <inheritdoc/>
    public unsafe void InitializeGraphics(ID3D11Device* device, IDXGISwapChain1* swapChain)
    {
        //_rdp.AttachGraphics(device, swapChain);
    }

    private IBusDevice GetChildDevice(ulong offset, out ulong deviceOffset)
    {
        deviceOffset = 0;
        foreach (var device in _childDevices)
        {
            if (offset >= device.BusRangeBase && offset < device.BusRangeBase + device.BusRangeSize)
            {
                deviceOffset = offset - device.BusRangeBase;
                return device;
            }
        }

        throw new ArgumentOutOfRangeException(nameof(offset), $"Offset {offset:X} is out of range for device {Name}");
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var device in _childDevices)
            device.Dispose();
    }
}
