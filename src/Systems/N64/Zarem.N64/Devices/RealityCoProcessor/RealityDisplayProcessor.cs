// Avishai Dernis 2026

using System;
using System.Buffers.Binary;
using Zarem.Emulator.Devices.MemoryBacking;
using Zarem.Emulator.Extensions;
using Zarem.N64.Devices.RealityCoProcessor.Enums.Registers;

namespace Zarem.N64.Devices.RealityCoProcessor;

#pragma warning disable CS0649

/// <summary>
/// A sub-components of the <see cref="RealityCoProcessor"/> responsible for processing the display operations.
/// </summary>
public partial class RealityDisplayProcessor : IRealityCoProcessorDevice
{
    private readonly FlatMemoryStore _regs;

    /// <summary>
    /// Initializes a new instance of the <see cref="RealityDisplayProcessor"/> class.
    /// </summary>
    public RealityDisplayProcessor()
    {
        _regs = new FlatMemoryStore(BusRangeSize);
    }

    /// <inheritdoc/>
    public string Name => "Reality Display Processor (RDP)";

    /// <inheritdoc/>
    public ulong BusRangeBase => 0x0410_0000;

    /// <inheritdoc/>
    public ulong BusRangeSize => 0x10_0010;

    /// <inheritdoc/>
    public void Read(ulong offset, Span<byte> destination) => _regs.Read(offset, destination);

    /// <inheritdoc/>
    public void Write(ulong offset, ReadOnlySpan<byte> source)
    {
        // Mask offset to the DPC register block range if accessed via standard offset mapping
        var regOffset = offset & 0xFF;
        var register = (RdpRegister)(uint)regOffset;
        uint value = BinaryPrimitives.ReadUInt32BigEndian(source);

        switch (register)
        {
            // Writing to DPC_END_REG triggers command processing execution
            case RdpRegister.End:
                _regs.Write(offset, source);
                ProcessCommandList();
                break;

            // Handle status flag modifications or control bit side-effects
            case RdpRegister.Status:
                HandleStatusWrite(value);
                break;

            // Fallback: Perform standard raw write for normal registers (Start, Current, etc.)
            default:
                _regs.Write(offset, source);
                break;
        }
    }

    private void ProcessCommandList()
    {
        uint start = _regs.Read<uint>((ulong)RdpRegister.Start);
        uint end = _regs.Read<uint>((ulong)RdpRegister.End);

        // Update current pointer and begin command processing loop
        _regs.Write((ulong)RdpRegister.Current, start);

        // TODO: Hook up command packet parser / decoder here

        _regs.Write((ulong)RdpRegister.Current, end);
    }

    private void HandleStatusWrite(uint value)
    {
        // Process DPC status register control bit alterations
        uint currentStatus = _regs.Read<uint>((ulong)RdpRegister.Status);

        // Example bit manipulation: clearing or setting status flags based on value bits
        // _regs.WriteUInt32((ulong)DpcRegister.Status, updatedStatus);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
    }
}
