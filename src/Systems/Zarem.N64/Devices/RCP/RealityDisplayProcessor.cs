// Avishai Dernis 2026

using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using System.Buffers.Binary;
using Zarem.Emulator.Machine;
using Zarem.N64.Devices.RCP.Enums;

namespace Zarem.N64.Devices.RCP;

/// <summary>
/// A sub-components of the <see cref="RealityCoProcessor"/> responsible for processing the display operations.
/// </summary>
public unsafe partial class RealityDisplayProcessor
{
    private readonly PhysicalBus _bus;
    private readonly RegisterFile<uint> _registerFile;

    private ComPtr<ID3D11Device> _device;
    private ComPtr<IDXGISwapChain1> _swapChain;

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
    /// Attaches the DirectX 11 device and swap chain to the RDP for rendering output.
    /// </summary>
    public void AttachGraphics(ID3D11Device* device, IDXGISwapChain1* swapChain)
    {
        _device = new ComPtr<ID3D11Device>(device);
        _swapChain = new ComPtr<IDXGISwapChain1>(swapChain);
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

    private void ProcessCommandList()
    {
        // Important: The RDP can only run if it's not "Frozen" via the Status register
        while (Current < End)
        {
            // Fetch the first word (the OpCode is in the most significant byte)
            var firstWord = _bus.Read<ulong>(Current);
            byte opCode = (byte)(firstWord >> 56);

            // Identify length and execute
            ExecuteCommand(opCode, Current, firstWord, out var commandSize);

            // Advance Current
            Current += commandSize;
        }
    }

    private void ExecuteCommand(byte opCode, uint address, ulong word0, out uint size)
    {
        size = 8;
        /*
        switch (opCode)
        {
            // --- 1 Word Commands (8 Bytes) ---
            case >= 0x29 and <= 0x3F:
                size = 8;
                HandleStateCommand(opCode, word0);
                break;

            // --- Rectangle Commands (Variable) ---
            case 0x24: // TEXTURE_RECTANGLE
            case 0x25: // TEXTURE_RECTANGLE_FLIP
                size = 24; // These are always 3 words (24 bytes)
                DrawTextureRectHLE(address);
                break;

            case 0x30: // FILL_RECTANGLE
                size = 8;
                DrawFillRectHLE(word0);
                break;

            // --- Triangle Commands (Dynamic Size) ---
            case >= 0x08 and <= 0x0F:
                size = CalculateTriangleSize(opCode);
                DrawTriangleHLE(opCode, address, size);
                break;

            // --- Sync/No-op Commands ---
            case 0x26: // PIPE_SYNC
            case 0x27: // TILE_SYNC
            case 0x28: // FULL_SYNC
                size = 8;
                // Handle synchronization logic for DirectX fence/flush if needed
                break;

            default:
                size = 8; // Default to 1 word to skip unknown data
                break;
        }
        */
    }

    private static uint CalculateTriangleSize(byte opCode)
    {
        // Base triangle is 1 word for coefficients + 3 words for edges = 4 words (32 bytes)
        uint words = 4;

        if ((opCode & 0x04) != 0) words += 4; // Shading (LRGBA) adds 4 words
        if ((opCode & 0x02) != 0) words += 4; // Texture (STW) adds 4 words
        if ((opCode & 0x01) != 0) words += 4; // Z-Buffer (Z) adds 4 words

        return words * 8;
    }
}
