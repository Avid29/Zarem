// Avishai Dernis 2026

using System;
using System.Buffers.Binary;
using Zarem.Emulator.Machine.Memory;
using Zarem.N64.Devices.RealityCoProcessor.Abstract;
using Zarem.N64.Models.Enums.Registers;

namespace Zarem.N64.Devices.RealityCoProcessor;

/// <summary>
/// A sub-components of the <see cref="RealityCoProcessor"/> responsible for ...
/// </summary>
public class N64SerialInterface : N64DmaDeviceBase
{
    private uint _dramAddress;
    private uint _status;

    /// <summary>
    /// Initializes a new instance of the <see cref="N64SerialInterface"/> class.
    /// </summary>
    public N64SerialInterface(PhysicalBus bus) : base(bus)
    {
    }

    /// <inheritdoc/>
    public override string Name => "N64 Serial Interface (SI)";

    /// <inheritdoc/>
    public override ulong BusRangeBase => 0x0480_0000;

    /// <inheritdoc/>
    public override ulong BusRangeSize => 0x20;

    /// <inheritdoc/>
    public override void Read(ulong offset, Span<byte> destination)
    {
        var register = (SiRegister)offset;
        uint value = register switch
        {
            SiRegister.DramAddress => _dramAddress,
            SiRegister.Status => _status,
            _ => 0,
        };

        BinaryPrimitives.WriteUInt32BigEndian(destination, value);
    }

    /// <inheritdoc/>
    public override void Write(ulong offset, ReadOnlySpan<byte> source)
    {
        var register = (SiRegister)offset;
        uint value = BinaryPrimitives.ReadUInt32BigEndian(source);

        switch (register)
        {
            case SiRegister.DramAddress:
                _dramAddress = value & 0x00FF_FFFF;
                break;

            case SiRegister.PifAddrRd64b: // PIF-RAM -> RDRAM
                ExecuteDmaTransfer(_dramAddress, N64PeripheralInterface.RamAbsoluteBase, N64PeripheralInterface.RamSize, writeToDram: true);
                break;

            case SiRegister.PifAddrWr64b: // RDRAM -> PIF-RAM
                ExecuteDmaTransfer(_dramAddress, N64PeripheralInterface.RamAbsoluteBase, N64PeripheralInterface.RamSize, writeToDram: false);
                break;

            case SiRegister.Status:
                _status &= ~2u;
                break;
        }
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
    }

    /// <inheritdoc/>
    protected override void SetBusyState(bool busy)
    {
        if (busy)
        {
            _status |= 0x1; // Set DMA busy bit
        }
        else
        {
            _status &= ~0x1u; // Clear DMA busy bit
        }
    }
}
