// Avishai Dernis 2026

using CommunityToolkit.HighPerformance;
using System;
using System.Buffers.Binary;
using Zarem.Emulator.Devices.MemoryBacking;
using Zarem.Emulator.Extensions;
using Zarem.Emulator.Machine.Memory;
using Zarem.N64.Devices.Abstract;

namespace Zarem.N64.Devices.RealitySignalProcessor;

/// <summary>
/// A sub-components of the <see cref="RealityCoProcessor"/> responsible for ...
/// </summary>
public partial class N64RealitySignalProcessor : N64DmaDeviceBase
{
    private const ulong DataMemorySize = 0x1000;
    private const ulong InstructionMemoryOffset = 0x1000;
    private const ulong InstructionMemorySize = 0x1000;
    private const ulong RegisterOffset = 0x4_0000;
    private const ulong RegisterSize = 0x20;

    private readonly FlatMemoryStore _dMem;
    private readonly FlatMemoryStore _iMem;
    private readonly FlatMemoryStore _regs;

    /// <summary>
    /// Initializes a new instance of the <see cref="N64RealitySignalProcessor"/> class.
    /// </summary>
    public N64RealitySignalProcessor(PhysicalBus bus) : base(bus)
    {
        _dMem = new(DataMemorySize);
        _iMem = new(InstructionMemorySize);
        _regs = new(RegisterSize);

        // Initialize SP_STATUS_REG (Offset 0x10 in the register block) as halted (bit 0 set)
        // This is done with command 0x2 (set halt)
        HandleStatusRegisterWrite(0x2);
    }

    /// <inheritdoc/>
    public override string Name => "Reality Signal Processor (RSP)";

    /// <inheritdoc/>
    public override ulong BusRangeBase => 0x0400_0000;

    /// <inheritdoc/>
    public override ulong BusRangeSize => 0x4_0020;

    /// <inheritdoc/>
    public override void Read(ulong offset, Span<byte> destination)
    {
        var childDevice = GetChildDevice(offset, out var deviceOffset);
        childDevice?.Read(deviceOffset, destination);
    }

    /// <inheritdoc/>
    public override void Write(ulong offset, ReadOnlySpan<byte> source)
    {
        var childDevice = GetChildDevice(offset, out var deviceOffset);
        if (childDevice is null)
            return;

        // Handle side effects and command registers first (bypassing raw store for commands)
        if (childDevice == _regs && source.Length >= 4)
        {
            var register = (RspRegister)deviceOffset;
            var value = BinaryPrimitives.ReadUInt32BigEndian(source);

            switch (register)
            {
                // Handle status register side effects
                case RspRegister.Status:
                    HandleStatusRegisterWrite(value);
                    return;
                    
                // Initiate DMA transfer
                case RspRegister.ReadLength:
                case RspRegister.WriteLength:
                    ExecuteDmaTransfer(value, register is RspRegister.ReadLength);
                    break;

                // TODO: Handle semaphore lock/clear
                case RspRegister.Semaphore:
                    return;
            }
        }

        // Fallback: Perform standard raw write for normal storage registers, DMEM, or IMEM
        childDevice.Write(deviceOffset, source);
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        _dMem.Dispose();
        _iMem.Dispose();
        _regs.Dispose();
    }

    /// <inheritdoc/>
    protected override void SetBusyState(bool busy)
    {
        // SP_STATUS_REG is at offset 0x10. Bit 2 (0x4) is the DMA Busy flag.
        uint currentStatus = _regs.Read<uint>((int)RspRegister.Status);

        if (busy)
        {
            currentStatus |= 0x4; // Set DMA busy bit
        }
        else
        {
            currentStatus &= ~0x4u; // Clear DMA busy bit
        }

        _regs.Write((int)RspRegister.Status, currentStatus);

        // Keep SP_DMA_BUSY_REG (offset 0x18) in sync if read directly elsewhere
        _regs.Write((int)RspRegister.DmaBusy, busy ? 1u : 0u);
    }

    private FlatMemoryStore? GetChildDevice(ulong offset, out ulong deviceOffset)
    {
        FlatMemoryStore? device;
        (device, deviceOffset) = offset switch
        {
            >= 0 and < DataMemorySize => (_dMem, offset),
            >= InstructionMemoryOffset and < InstructionMemoryOffset + InstructionMemorySize => (_iMem, offset - InstructionMemoryOffset),
            >= RegisterOffset and < RegisterOffset + RegisterSize => (_regs, offset - RegisterOffset),
            _ => (null, offset),
        };
        return device;
    }

    private void ExecuteDmaTransfer(uint command, bool writeToDram)
    {
        // N64 DMA length registers encode length and count/skip parameters:
        uint transferLength = (command & 0xFFF) + 1;
        uint count = ((command >> 12) & 0xFF) + 1;
        uint skip = (command >> 20) & 0xFFF;

        // Retreive transfer start addresses
        uint rspMemAddress = _regs.Read<uint>((int)RspRegister.MemoryAddress) & 0x1FFF;
        uint dramAddress = _regs.Read<uint>((int)RspRegister.DramAddress) & 0x00FFFFFF;
        rspMemAddress += (uint)BusRangeBase;

        ExecuteDmaTransfer(dramAddress, rspMemAddress, transferLength, writeToDram, count, skip);
    }

    private void HandleStatusRegisterWrite(uint command)
    {
        // Fetch status value
        uint currentStatus = _regs.Read<uint>((int)RspRegister.Status);

        if ((command & 0x0001) != 0) currentStatus &= ~0x0001u;     // Command Bit 0: Clear Halt
        if ((command & 0x0002) != 0) currentStatus |= 0x0001u;      // Command Bit 1: Set Halt
        if ((command & 0x0004) != 0) currentStatus &= ~0x0002u;     // Command Bit 2: Clear Broke
        if ((command & 0x0008) != 0) { /* TODO: */ }                // Command Bit 3: Clear Interrupt
        if ((command & 0x0010) != 0) { /* TODO: */ }                // Command Bit 4: Set Interrupt
        if ((command & 0x0020) != 0) currentStatus &= ~0x0020u;     // Command Bit 5: Clear Single Step Mode
        if ((command & 0x0040) != 0) currentStatus |= 0x0020u;      // Command Bit 6: Set Single Step Mode
        if ((command & 0x0080) != 0) currentStatus &= ~0x0040u;     // Command Bit 7: Clear Interrupt on Break
        if ((command & 0x0100) != 0) currentStatus |= 0x0040u;      // Command Bit 8: Set Interrupt on Break

        // Writeback new status value
        _regs.Write((int)RspRegister.Status, currentStatus);
    }
}
