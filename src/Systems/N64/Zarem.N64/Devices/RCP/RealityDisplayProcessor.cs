// Avishai Dernis 2026

using System;
using System.Buffers.Binary;
using Zarem.Emulator.Machine.Memory;
using Zarem.Emulator.Machine.Registers;
using Zarem.N64.Devices.RCP.Enums;

namespace Zarem.N64.Devices.RCP;

#pragma warning disable CS0649

/// <summary>
/// A sub-components of the <see cref="RealityCoProcessor"/> responsible for processing the display operations.
/// </summary>
public partial class RealityDisplayProcessor
{
    private readonly PhysicalBus _bus;
    private readonly RegisterFile<uint> _registerFile;

    /// <summary>
    /// Initializes a new instance of the <see cref="RealityDisplayProcessor"/> class.
    /// </summary>
    public RealityDisplayProcessor(PhysicalBus bus)
    {
        _bus = bus;
        _registerFile = new(8);
    }

    /// <summary>
    /// Gets or sets the start register value.
    /// </summary>
    public uint Start
    {
        get => this[RdpRegister.Start];
        set => this[RdpRegister.Start] = value;
    }

    /// <summary>
    /// Gets or sets the end register value.
    /// </summary>
    public uint End
    {
        get => this[RdpRegister.End];
        set => this[RdpRegister.End] = value;
    }

    /// <summary>
    /// Gets or sets the current register value.
    /// </summary>
    public uint Current
    {
        get => this[RdpRegister.Current];
        set => this[RdpRegister.Current] = value;
    }

    /// <summary>
    /// Gets or sets the status register value.
    /// </summary>
    public uint Status
    {
        get => this[RdpRegister.Status];
        set => this[RdpRegister.Status] = value;
    }

    /// <summary>
    /// Gets or sets a register in the <see cref="RealityDisplayProcessor"/> register file.
    /// </summary>
    public uint this[RdpRegister reg]
    {
        get => _registerFile[(int)reg];
        set => _registerFile[(int)reg] = value;
    }

    /// <summary>
    /// Writes the specified data at the given offset to the RDP registers.
    /// </summary>
    public void WriteRegister(ulong offset, ReadOnlySpan<byte> data)
    {
        // N64 registers are 32-bit (4 bytes)
        int regIndex = (int)(offset / 4);

        // Update the register file
        uint value = BinaryPrimitives.ReadUInt32BigEndian(data);
        var register = (RdpRegister)regIndex;
        _registerFile[regIndex] = value;

        // Handle any side effects of writing to the register.
        switch (register)
        {
            // Writing to the Start register sets the Current register to the same value.
            case RdpRegister.Start:
                Current = Start;
                break;

            // Writing to the End register triggers the processing of the command list from Start to End.
            case RdpRegister.End:
                ProcessCommandList();
                break;
        }
    }
}
