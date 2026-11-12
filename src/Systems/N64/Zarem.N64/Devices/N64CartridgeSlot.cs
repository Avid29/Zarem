// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Devices.Interfaces;
using Zarem.Emulator.Devices.MemoryBacking;

namespace Zarem.N64.Devices;

/// <summary>
/// A <see cref="IDevice"/> that behaves as a N64 game cartridge.
/// </summary>
public unsafe class N64CartridgeSlot : IBusDeviceDirect
{
#pragma warning disable CS1591
    public const ulong BusRangeBase = 0x1000_0000;
    private const ulong RangeSize = 0x0100_0000;
#pragma warning restore CS1591

    private FileBackedMemoryStore? _memoryStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="N64CartridgeSlot"/> class.
    /// </summary>
    public N64CartridgeSlot(ulong maxSize = RangeSize)
    {
        UnloadCartridge();

        BusRangeSize = maxSize;
    }

    /// <inheritdoc/>
    public ulong BusRangeSize { get; }

    /// <inheritdoc/>
    public string Name => "N64 Cartridge";

    /// <summary>
    /// Loads a z64 file as an N64 cartridge.
    /// </summary>
    public void LoadCartridge(string z64FilePath)
    {
        // Unload the current cartidge
        UnloadCartridge();

        _memoryStore = new FileBackedMemoryStore(z64FilePath, BusRangeSize, MemoryMappedFileAccess.Read);
    }

    /// <summary>
    /// Unloads the current z64 cartridge.
    /// </summary>
    public void UnloadCartridge()
    {
        _memoryStore?.Dispose();
        _memoryStore = null;
    }

    /// <inheritdoc/>
    public void Read(ulong offset, Span<byte> destination) => _memoryStore?.Read(offset, destination);

    /// <inheritdoc/>
    public void Write(ulong offset, ReadOnlySpan<byte> source)
    {
        throw new NotSupportedException("Cartridge memory is read-only.");
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte* GetPointer(ulong offset)
    {
        Guard.IsNotNull(_memoryStore, "Cartridge is not loaded.");
        Guard.IsLessThan(offset, _memoryStore.Size);
        return _memoryStore.GetPointer(offset);
    }

    /// <inheritdoc/>
    public void Dispose() => UnloadCartridge();
}
