// Avishai Dernis 2026

using System;
using System.Buffers.Binary;
using Zarem.Emulator.Devices.MemoryBacking;
using Zarem.Emulator.Extensions;
using Zarem.N64.Devices.RealityCoProcessor.Abstract;
using Zarem.N64.Models.Enums.Registers;

namespace Zarem.N64.Devices.RealityCoProcessor;

/// <summary>
/// A sub-component of the <see cref="RealityCoProcessor"/> responsible for handling MIPS Interface (MI) control, version, and interrupt routing.
/// </summary>
public class N64MipsInterface : IRealityCoProcessorDevice
{
    private readonly FlatMemoryStore _regs;

    /// <summary>
    /// Initializes a new instance of the <see cref="N64MipsInterface"/> class.
    /// </summary>
    public N64MipsInterface()
    {
        _regs = new FlatMemoryStore(BusRangeBase);

        // Initialize standard default version (e.g., standard N64 MI version ID)
        _regs.Write((ulong)MiRegister.Version, 0x0202_0102);
    }

    /// <inheritdoc/>
    public string Name => "N64 MIPS Interface (MI)";

    /// <inheritdoc/>
    public ulong BusRangeBase => 0x0430_0000;

    /// <inheritdoc/>
    public ulong BusRangeSize => 0x10;

    /// <inheritdoc/>
    public void Read(ulong offset, Span<byte> destination)
    {
        if (destination.Length != 4)
            throw new ArgumentException("MI registers are 32-bit width.", nameof(destination));

        _regs.Read(offset, destination);
    }

    /// <inheritdoc/>
    public void Write(ulong offset, ReadOnlySpan<byte> source)
    {
        if (source.Length != 4)
            throw new ArgumentException("MI registers are 32-bit width.", nameof(source));

        var register = (MiRegister)(uint)(offset & 0xFF);
        uint value = BinaryPrimitives.ReadUInt32BigEndian(source);

        switch (register)
        {
            case MiRegister.Mode:
                HandleModeWrite(value);
                break;

            case MiRegister.InterruptMask:
                HandleInterruptMaskWrite(value);
                break;

            default:
                _regs.Write(offset, source);
                break;
        }
    }

    private void HandleModeWrite(uint value)
    {
        // MI_MODE_REG bit operations (e.g., setting/clearing init mode, EBRAM size, etc.)
        // Bit 7: clear init mode, Bit 8: set init mode, etc.
        uint currentMode = _regs.Read<uint>((ulong)MiRegister.Mode);

        // Apply bit modifications per N64 hardware specs if needed
        _regs.Write((ulong)MiRegister.Mode, value);
    }

    private void HandleInterruptMaskWrite(uint value)
    {
        // MI_INTR_MASK_REG handles setting and clearing mask bits via individual bits
        // Bits 0-5 set/clear masks for SP, SI, AI, VI, PI, DP interrupts
        _regs.Write((ulong)MiRegister.InterruptMask, value);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
    }
}
