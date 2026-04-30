// Avishai Dernis 2026

using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using Zarem.Emulator.Devices.Interfaces;
using Zarem.Emulator.Machine;

namespace Zarem.N64.Devices.RCP;

/// <summary>
/// An <see cref="IBusDevice"/> implementation for the N64 reality coprocessor (rcp) graphics card.
/// </summary>
public class RealityCoProcessor : IBusDevice, IDXGraphicsDevice
{
#pragma warning disable CS1591
    public const ulong RspDataMemoryBase = 0x0;
    public const ulong RspDataMemorySize = 0x1000;
    public const ulong RspIntstructionMemoryBase = 0x1000;
    public const ulong RspIntstructionMemorySize = 0x1000;
    public const ulong RspRegistersBase = 0x4_1000;
    public const ulong RspRegistersSize = 0x20;
    public const ulong RdpRegistersBase = 0x10_0000;
    public const ulong RdpRegistersSize = 0x20;
    public const ulong ViRegistersBase = 0x40_0000;
    public const ulong ViRegistersSize = 0x3C;
#pragma warning restore CS1591

    private readonly RealitySignalProcessor _rsp;
    private readonly RealityDisplayProcessor _rdp;
    private readonly N64VideoInterface _vi;

    /// <summary>
    /// Initializes a new instance of the <see cref="RealityCoProcessor"/> class.
    /// </summary>
    public RealityCoProcessor(PhysicalBus bus)
    {
        _rsp = new RealitySignalProcessor(bus);
        _rdp = new RealityDisplayProcessor(bus);
        _vi = new N64VideoInterface();
    }

    /// <inheritdoc/>
    public string Name => "Reality CoProcessor";

    /// <inheritdoc/>
    public ulong BusRangeSize => Nintendo64.RcpSize;

    /// <inheritdoc/>
    public void Read(ulong offset, Span<byte> destination)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void Write(ulong offset, ReadOnlySpan<byte> source)
    {
        if (offset is >= RspDataMemoryBase and < RspIntstructionMemoryBase + RspIntstructionMemorySize)
        {
            // RSP Data/Instruction Memory
            _rsp.WriteMemory(offset - RspDataMemoryBase, source);
        }
        else if (offset is >= RspRegistersBase and < RspRegistersBase + RspRegistersSize)
        {
            // RSP Registers
            _rsp.WriteRegister(offset - RspRegistersBase, source);
        }
        else if (offset is >= RdpRegistersBase and < RdpRegistersBase + RdpRegistersSize)
        {
            // RDP Registers
            _rdp.WriteRegister(offset - RdpRegistersBase, source);
        }
        else if (offset is >= ViRegistersBase and < ViRegistersBase + ViRegistersSize)
        {
            // VI Registers
            _vi.WriteRegister(offset - ViRegistersBase, source);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    /// <inheritdoc/>
    public unsafe void InitializeGraphics(ID3D11Device* device, IDXGISwapChain* swapChain)
    {
        _rdp.AttachGraphics(device, swapChain);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _rsp.Dispose();
    }
}
