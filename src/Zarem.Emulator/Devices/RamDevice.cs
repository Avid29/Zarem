// Avishai Dernis 2026

using System;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Devices.Interfaces;
using Zarem.Emulator.Devices.MemoryBacking;

namespace Zarem.Emulator.Devices;

/// <summary>
/// An <see cref="IBusDevice"/> that is the system RAM.
/// </summary>
public unsafe class RamDevice: IBusDeviceDirect
{
    private readonly IMemoryBackingStore _memoryStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="RamDevice"/> class.
    /// </summary>
    public RamDevice(IMemoryBackingStore memoryStore)
    {
        _memoryStore = memoryStore;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RamDevice"/> class.
    /// </summary>
    public RamDevice(ulong size, uint pageSize = 4096)
    {
        _memoryStore = size switch
        {
            // For small RAM sizes (16KB or less), use a flat memory store for performance
            < 0x4_000 => new FlatMemoryStore(size),

            // For larger RAM sizes, use a paged memory store to conserve memory
            _ => new PagedMemoryStore(size, pageSize),
        };
    }

    /// <inheritdoc/>
    public virtual string Name => "RAM";

    /// <inheritdoc/>
    public virtual ulong BusRangeSize => _memoryStore.Size;

    /// <inheritdoc/>
    public virtual void Read(ulong offset, Span<byte> destination) => _memoryStore.Read(offset, destination);

    /// <inheritdoc/>
    public virtual void Write(ulong offset, ReadOnlySpan<byte> source) => _memoryStore.Write(offset, source);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual byte* GetPointer(ulong offset) => _memoryStore.GetPointer(offset);

    /// <inheritdoc/>
    public void Dispose() => _memoryStore.Dispose();
}
