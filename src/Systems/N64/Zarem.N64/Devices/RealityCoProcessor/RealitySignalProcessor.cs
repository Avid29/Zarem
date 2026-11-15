// Avishai Dernis 2026

using System;
using System.Buffers.Binary;
using Zarem.Emulator.Devices.MemoryBacking;
using Zarem.Emulator.Machine.Memory;

namespace Zarem.N64.Devices.RealityCoProcessor;

/// <summary>
/// A sub-components of the <see cref="RealityCoProcessor"/> responsible for ...
/// </summary>
public partial class RealitySignalProcessor : IRealityCoProcessorDevice
{
    private const ulong DataMemorySize = 0x1000;
    private const ulong InstructionMemoryOffset = 0x1000;
    private const ulong InstructionMemorySize = 0x1000;
    private const ulong RegisterOffset = 0x4_0000;
    private const ulong RegisterSize = 0x20;

    private readonly FlatMemoryStore _dMem;
    private readonly FlatMemoryStore _iMem;
    private readonly FlatMemoryStore _regs;
    private readonly PhysicalBus _bus;

    /// <summary>
    /// Initializes a new instance of the <see cref="RealitySignalProcessor"/> class.
    /// </summary>
    public RealitySignalProcessor(PhysicalBus bus)
    {
        _bus = bus;
        _dMem = new(DataMemorySize);
        _iMem = new(InstructionMemorySize);
        _regs = new(RegisterSize);

        // Initialize SP_STATUS_REG (Offset 0x10 in the register block) as halted (bit 0 set)
        // This is done with command 0x2 (set halt)
        HandleStatusRegisterWrite(0x2);
    }

    /// <inheritdoc/>
    public string Name => "Reality Signal Processor (RSP)";

    /// <inheritdoc/>
    public ulong BusRangeBase => 0x0400_0000;

    /// <inheritdoc/>
    public ulong BusRangeSize => 0x4_0020;

    /// <inheritdoc/>
    public void Read(ulong offset, Span<byte> destination)
    {
        var childDevice = GetChildDevice(offset, out var deviceOffset);
        childDevice?.Read(deviceOffset, destination);
    }

    /// <inheritdoc/>
    public void Write(ulong offset, ReadOnlySpan<byte> source)
    {
        var childDevice = GetChildDevice(offset, out var deviceOffset);
        if (childDevice is null)
            return;

        // Handle side effects and command registers first (bypassing raw store for commands)
        if (childDevice == _regs && source.Length >= 4)
        {
            var register = (RspRegister)deviceOffset;

            // Check if this register requires command parsing / side-effects instead of raw storage
            if (register is RspRegister.Status
                or RspRegister.DmaReadLength
                or RspRegister.DmaWriteLength
                or RspRegister.Semaphore)
            {
                var value = BinaryPrimitives.ReadUInt32BigEndian(source);
                CommandRegisterWrite(register, value);
                return;
            }
        }

        // Fallback: Perform standard raw write for normal storage registers, DMEM, or IMEM
        childDevice.Write(deviceOffset, source);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _dMem.Dispose();
        _iMem.Dispose();
        _regs.Dispose();
    }

    private FlatMemoryStore? GetChildDevice(ulong offset, out ulong deviceOffset)
    {
        FlatMemoryStore? device;
        (device, deviceOffset) = offset switch
        {
            > 0 and < DataMemorySize => (_dMem, offset),
            > InstructionMemoryOffset and < InstructionMemoryOffset + InstructionMemorySize => (_iMem, offset - InstructionMemoryOffset),
            > RegisterOffset and < RegisterOffset + RegisterSize => (_regs, offset - RegisterOffset),
            _ => (null, offset),
        };
        return device;
    }
}
