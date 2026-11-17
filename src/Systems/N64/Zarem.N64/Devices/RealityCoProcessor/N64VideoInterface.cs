// Avishai Dernis 2026

using System;
using System.Buffers.Binary;
using Zarem.Emulator.Devices.MemoryBacking;
using Zarem.N64.Devices.RealityCoProcessor.Abstract;
using Zarem.N64.Models.Enums.Registers;

namespace Zarem.N64.Devices.RealityCoProcessor;

/// <summary>
/// A sub-components of the <see cref="RealityCoProcessor"/> responsible for output and timing.
/// </summary>
public class N64VideoInterface : IRealityCoProcessorDevice
{
    private readonly FlatMemoryStore _regs;

    /// <summary>
    /// Initializes a new instance of the <see cref="N64VideoInterface"/> class.
    /// </summary>
    public N64VideoInterface()
    {
        _regs = new FlatMemoryStore(BusRangeSize);
    }

    /// <inheritdoc/>
    public string Name => "N64 Video Interface (VI)";

    /// <inheritdoc/>
    public ulong BusRangeBase => 0x0440_0000;

    /// <inheritdoc/>
    public ulong BusRangeSize => 0x40;

    /// <inheritdoc/>
    public void Read(ulong offset, Span<byte> destination) => _regs.Read(offset, destination);

    /// <inheritdoc/>
    public void Write(ulong offset, ReadOnlySpan<byte> source)
    {
        if (source.Length < 4)
            return;

        var register = (ViRegister)(uint)offset;
        uint value = BinaryPrimitives.ReadUInt32BigEndian(source);

        // Handle special register write side effects
        switch (register)
        {
            // TODO: Handle VI Register write side-effects

            default:
                break;
        }

        // Perform standard storage write
        _regs.Write(offset, source);
    }

    /// <inheritdoc/>
    public void Dispose() => _regs.Dispose();
}
