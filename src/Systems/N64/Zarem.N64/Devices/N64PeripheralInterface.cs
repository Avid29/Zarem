// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Devices.Interfaces;
using Zarem.Emulator.Devices.MemoryBacking;

namespace Zarem.N64.Devices;

/// <summary>
/// An <see cref="IBusDeviceDirect"/> implementation for the N64 Peripheral Interface (PIF).
/// </summary>
public unsafe class N64PeripheralInterface : IBusDeviceDirect
{
#pragma warning disable CS1591
    public const ulong BusRangeBase = 0x1FC0_0000;
    private const ulong RangeSize = 0x0800; // 2 KB total window
    private const ulong RomSize = 0x7C0;      // 1984 bytes
    private const ulong RamOffset = 0x7C0;
    private const ulong RamSize = 0x40;       // 64 bytes
#pragma warning restore CS1591

    private readonly FileBackedMemoryStore _romStore;
    private readonly FlatMemoryStore _ramStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="N64PeripheralInterface"/> class.
    /// </summary>
    /// <param name="bootRomPath"></param>
    public N64PeripheralInterface(string bootRomPath)
    {
        _romStore = new(bootRomPath, RomSize, MemoryMappedFileAccess.Read);
        _ramStore = new(RamSize);
    }

    /// <inheritdoc/>
    public string Name => "PIF (Boot ROM & RAM)";

    /// <inheritdoc/>
    public ulong BusRangeSize => RangeSize;

    /// <inheritdoc/>
    public void Read(ulong offset, Span<byte> destination)
    {
        switch (offset)
        {
            case < RomSize:
                // Handle reads that might span across the ROM/RAM boundary if necessary,
                // or dispatch directly if contained within a single region.
                _romStore.Read(offset, destination);
                break;
            case < RamOffset + RamSize:
                _ramStore.Read(offset - RamOffset, destination);
                break;
            default:
                ThrowHelper.ThrowArgumentOutOfRangeException($"Offset {offset:X} is out of range for PIF device.");
                break;
        }
    }

    /// <inheritdoc/>
    public void Write(ulong offset, ReadOnlySpan<byte> source)
    {
        switch (offset)
        {
            case < RomSize:
                // Writes to the Boot ROM region are ignored
                break;
            case < RamOffset + RamSize:
                _ramStore.Write(offset - RamOffset, source);
                break;
            default:
                ThrowHelper.ThrowArgumentOutOfRangeException($"Offset {offset:X} is out of range for PIF device.");
                break;
        }
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte* GetPointer(ulong offset)
    {
        return offset switch
        {
            < RomSize => _romStore.GetPointer(offset),
            < RamOffset + RamSize => _ramStore.GetPointer(offset - RamOffset),
            _ => (byte*)ThrowHelper.ThrowArgumentOutOfRangeException<nint>($"Offset {offset:X} is out of range for PIF device."),
        };
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _romStore.Dispose();
        _ramStore.Dispose();
    }
}
