// Avishai Dernis 2026

using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Zarem.Emulator.Machine;
using Zarem.Extensions;
using Zarem.N64.Devices.RCP.Enums;

namespace Zarem.N64.Devices.RCP;

/// <summary>
/// A sub-components of the <see cref="RealityCoProcessor"/> responsible for processing the DMA transfers to/from the RCP.
/// </summary>
public unsafe class RealitySignalProcessor : IDisposable
{
    private readonly PhysicalBus _bus;

    private readonly RegisterFile<uint> _registerFile;
    private readonly byte* _memory;
    private readonly byte* _dataMemory;
    private readonly byte* _instructionMemory;

    /// <summary>
    /// Initializes a new instance of the <see cref="RealitySignalProcessor"/> class.
    /// </summary>
    public RealitySignalProcessor(PhysicalBus bus)
    {
        _bus = bus;

        _registerFile = new(4);
        _memory = (byte*)NativeMemory.Alloc((nuint)(RealityCoProcessor.RspDataMemorySize + RealityCoProcessor.RspIntstructionMemorySize));
        _dataMemory = _memory + RealityCoProcessor.RspDataMemoryBase;
        _instructionMemory = _memory + RealityCoProcessor.RspIntstructionMemoryBase;
    }

    /// <summary>
    /// Gets or sets the DMA dram address register value.
    /// </summary>
    public uint DramAddress
    {
        get => this[RspRegister.DramAddress];
        set => this[RspRegister.DramAddress] = value;
    }

    /// <summary>
    /// Gets or sets the DMA write address register value.
    /// </summary>
    public uint RspMemoryAddress
    {
        get => this[RspRegister.RspMemoryAddress];
        set => this[RspRegister.RspMemoryAddress] = value;
    }

    /// <summary>
    /// Gets or sets the DMA read length register value.
    /// </summary>
    public uint ReadLength
    {
        get => this[RspRegister.ReadLength];
        set => this[RspRegister.ReadLength] = value;
    }

    /// <summary>
    /// Gets or sets the DMA write length register value.
    /// </summary>
    public uint WriteLength
    {
        get => this[RspRegister.WriteLength];
        set => this[RspRegister.WriteLength] = value;
    }

    /// <summary>
    /// Gets or sets a register in the <see cref="RealitySignalProcessor"/> register file.
    /// </summary>
    public uint this[RspRegister register]
    {
        get => _registerFile[(int)register];
        set => _registerFile[(int)register] = value;
    }

    /// <summary>
    /// Writes the specified data to memory at the given offset.
    /// </summary>
    public void WriteMemory(ulong offset, ReadOnlySpan<byte> data)
    {
        if (offset < RealityCoProcessor.RspDataMemorySize)
        {
            // Write to RSP Data Memory
            ulong dataOffset = offset;
            Unsafe.CopyBlock(_dataMemory + dataOffset, data);
        }
        else if (offset >= RealityCoProcessor.RspDataMemorySize && offset < RealityCoProcessor.RspDataMemorySize + RealityCoProcessor.RspIntstructionMemorySize)
        {
            // Write to RSP Instruction Memory
            ulong instructionOffset = offset - RealityCoProcessor.RspDataMemorySize;
            Unsafe.CopyBlock(_instructionMemory + instructionOffset, data);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset is out of bounds for RSP memory.");
        }

    }

    /// <summary>
    /// Writes the specified data at the given offset to the RSP registers.
    /// </summary>
    public void WriteRegister(ulong offset, ReadOnlySpan<byte> data)
    {
        // N64 registers are 32-bit (4 bytes)
        int regIndex = (int)(offset / 4);

        // Update the register file
        uint value = BinaryPrimitives.ReadUInt32BigEndian(data);
        var register = (RspRegister)regIndex;
        _registerFile[regIndex] = value;

        // Handle any side effects of writing to the register.
        switch (register)
        {
            case RspRegister.ReadLength:
                ExecuteReadDma();
                break;
            case RspRegister.WriteLength:
                ExecuteWriteDma();
                break;
        }
    }

    private void ExecuteReadDma()
    {
        var count = ReadLength;
        var dest = new Span<byte>(_memory, (int)count);
        _bus.Read(DramAddress, dest);

        DramAddress += count;
        RspMemoryAddress += count;
    }

    private void ExecuteWriteDma()
    {
        var count = WriteLength;
        var source = new Span<byte>(_memory, (int)count);
        _bus.Write(DramAddress, source);

        DramAddress += count;
        RspMemoryAddress += count;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_memory is not null)
        {
            NativeMemory.Free(_memory);
        }
    }
}
