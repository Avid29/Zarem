// Avishai Dernis 2026

using System;
using System.Buffers.Binary;
using Zarem.Emulator.Devices.MemoryBacking;
using Zarem.Emulator.Machine.Memory;
using Zarem.N64.Devices.RealityCoProcessor.Enums;

namespace Zarem.N64.Devices.RealityCoProcessor;

/// <summary>
/// A sub-components of the <see cref="RealityCoProcessor"/> responsible for ...
/// </summary>
public partial class N64ParallelInterface : IRealityCoProcessorDevice
{
    private readonly PhysicalBus _bus;
    private readonly FlatMemoryStore _regs;

    /// <summary>
    /// Initializes a new instance of the <see cref="N64ParallelInterface"/> class.
    /// </summary>
    public N64ParallelInterface(PhysicalBus bus)
    {
        _bus = bus;
        _regs = new FlatMemoryStore(BusRangeBase);
    }

    /// <inheritdoc/>
    public string Name => "N64 Parallel Interface (PI)";

    /// <inheritdoc/>
    public ulong BusRangeBase => 0x0460_0000;

    /// <inheritdoc/>
    public ulong BusRangeSize => 0x40;

    /// <inheritdoc/>
    public void Read(ulong offset, Span<byte> destination) => _regs.Read(offset, destination);

    /// <inheritdoc/>
    public void Write(ulong offset, ReadOnlySpan<byte> source)
    {
        uint value = BinaryPrimitives.ReadUInt32BigEndian(source);
        var reg = (PiRegister)(uint)offset;

        // Trigger hardware actions or mask bits where side effects occur
        switch (reg)
        {
            case PiRegister.ReadLength:
                ExcecuteDmaTransfer(value, false);
                break;

            case PiRegister.WriteLength:
                ExcecuteDmaTransfer(value, true);
                break;

            case PiRegister.Status:
                HandleStatusWrite(value);
                break;

            // Fallback: Perform standard raw write for normal storage registers
            default:
                _regs.Write(offset, source);
                break;
        }
    }

    /// <inheritdoc/>
    public void Dispose() => _regs.Dispose();
}
