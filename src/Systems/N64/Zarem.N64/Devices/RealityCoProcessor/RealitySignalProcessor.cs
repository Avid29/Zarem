// Avishai Dernis 2026

using System;
using System.Buffers.Binary;
using Zarem.Emulator.Devices.MemoryBacking;

namespace Zarem.N64.Devices.RealityCoProcessor;

/// <summary>
/// A sub-components of the <see cref="RealityCoProcessor"/> responsible for ...
/// </summary>
public class RealitySignalProcessor : IRealityCoProcessorDevice
{
    private const ulong DataMemorySize = 0x1000;
    private const ulong InstructionMemoryOffset = 0x1000;
    private const ulong InstructionMemorySize = 0x1000;
    private const ulong RegisterOffset = 0x2000;
    private const ulong RegisterSize = 0x20;

    private readonly FlatMemoryStore _dMem;
    private readonly FlatMemoryStore _iMem;
    private readonly FlatMemoryStore _regs;

    /// <summary>
    /// Initializes a new instance of the <see cref="RealitySignalProcessor"/> class.
    /// </summary>
    public RealitySignalProcessor()
    {
        // Initialize RAM regions and registers
        _dMem = new(DataMemorySize);
        _iMem = new(InstructionMemorySize);
        _regs = new(RegisterSize);

        // Initialize SP_STATUS_REG (Offset 0x10 in the register block) as halted (bit 0 set)
        Span<byte> initStatus = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(initStatus, 0x00000001);
        Write(RegisterOffset + 0x10, initStatus);
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
        childDevice?.Write(deviceOffset, source);

        if (childDevice == _regs)
        {
            // TODO: Handle register effects
        }
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
            < DataMemorySize => (_dMem, offset),
            < InstructionMemoryOffset + InstructionMemorySize => (_iMem, offset - InstructionMemoryOffset),
            < RegisterOffset + RegisterSize => (_regs, offset - RegisterOffset),
            _ => (null, offset),
        };
        return device;
    }
}
