// Avishai Dernis 2026

using System;
using System.Buffers.Binary;
using Zarem.Emulator.Devices.MemoryBacking;
using Zarem.Emulator.Extensions;
using Zarem.Emulator.Machine.Memory;
using Zarem.N64.Devices.RealityCoProcessor.Enums.Registers;

namespace Zarem.N64.Devices.RealityCoProcessor;

/// <summary>
/// A sub-components of the <see cref="RealityCoProcessor"/> responsible for ...
/// </summary>
public partial class N64ParallelInterface : N64DmaDeviceBase
{
    private readonly FlatMemoryStore _regs;

    /// <summary>
    /// Initializes a new instance of the <see cref="N64ParallelInterface"/> class.
    /// </summary>
    public N64ParallelInterface(PhysicalBus bus) : base(bus)
    {
        _regs = new FlatMemoryStore(BusRangeSize);
    }

    /// <inheritdoc/>
    public override string Name => "N64 Parallel Interface (PI)";

    /// <inheritdoc/>
    public override ulong BusRangeBase => 0x0460_0000;

    /// <inheritdoc/>
    public override ulong BusRangeSize => 0x40;

    /// <inheritdoc/>
    public override void Read(ulong offset, Span<byte> destination) => _regs.Read(offset, destination);

    /// <inheritdoc/>
    public override void Write(ulong offset, ReadOnlySpan<byte> source)
    {
        var register = (PiRegister)(uint)offset;
        uint value = BinaryPrimitives.ReadUInt32BigEndian(source);

        switch (register)
        {
            // Handle status register side effects
            case PiRegister.Status:
                HandleStatusWrite(value);
                break;

            // Initiate DMA transfer
            case PiRegister.ReadLength:
            case PiRegister.WriteLength:
                ExcecuteDmaTransfer(value, register is PiRegister.ReadLength);
                goto default;

            // Fallback: Perform standard raw write for normal storage registers
            default:
                _regs.Write(offset, source);
                break;
        }
    }

    /// <inheritdoc/>
    public override void Dispose() => _regs.Dispose();
    
    /// <inheritdoc/>
    protected override void SetBusyState(bool busy)
    {
        uint status = _regs.Read<uint>((int)PiRegister.Status);

        if (busy)
        {
            status |= 0x01;     // Set DMA Busy
        }
        else
        {
            status &= ~0x01U;   // Clear DMA busy
            status |= 0x04U;    // Set PI interrupt flag
        }

        _regs.Write((int)PiRegister.Status, status);
    }

    private void ExcecuteDmaTransfer(uint length, bool writeToDram)
    {
        // Read current 24-bit RDRAM address and 32-bit cartridge physical address
        uint dramAddress = _regs.Read<uint>((int)PiRegister.DramAddress) & 0x00FF_FFFF;
        uint cartridgeAddress = _regs.Read<uint>((int)PiRegister.CartridgeAddress);

        // N64 PI DMA length registers store (transfer_size - 1)
        uint transferLength = length + 1;

        ExecuteDmaTransfer(dramAddress, cartridgeAddress, transferLength, writeToDram);

        // Update address registers to point to the end of the transfer
        _regs.Write((int)PiRegister.DramAddress, (dramAddress + transferLength) & 0x00FF_FFFF);
        _regs.Write((int)PiRegister.CartridgeAddress, cartridgeAddress + transferLength);
    }

    private void HandleStatusWrite(uint value)
    {
        // Fetch status value
        uint currentStatus = _regs.Read<uint>((int)PiRegister.Status);

        // Modify status value
        if ((value & 0x1) != 0) currentStatus &= ~0x04U; // Clear PI interrupt
        if ((value & 0x2) != 0) currentStatus = 0;      // Reset PI controller

        // Writeback new status value
        _regs.Write((int)PiRegister.Status, currentStatus);
    }
}
