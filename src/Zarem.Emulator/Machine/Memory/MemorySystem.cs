// Avishai Dernis 2026

using System;
using System.IO;
using System.Numerics;
using Zarem.Emulator.Models.Enums;

namespace Zarem.Emulator.Machine.Memory;

/// <summary>
/// A class for a system handling physical and virtual memory in an <see cref="IComputer"/>.
/// </summary>
public sealed class MemorySystem : IMemorySystem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemorySystem"/> class.
    /// </summary>
    public MemorySystem(PhysicalBus physical, IAddressTranslator translator)
    {
        Physical = physical;
        Virtual = new VirtualMemorySystem(physical, translator);
    }

    /// <inheritdoc/>
    public PhysicalBus Physical { get; }

    /// <inheritdoc/>
    public IVirtualMemoryAccessor Virtual { get; }

    /// <inheritdoc/>
    public MemoryAccessResult TryRead<T>(ulong address, out T value)
        where T : unmanaged, IBinaryNumber<T>
        => Virtual.TryRead(address, out value);

    /// <inheritdoc/>
    public T Read<T>(ulong address)
        where T : unmanaged, IBinaryNumber<T>
        => Virtual.Read<T>(address);

    /// <inheritdoc/>
    public MemoryAccessResult TryRead(ulong address, Span<byte> buffer)
        => Virtual.TryRead(address, buffer);

    /// <inheritdoc/>
    public void Read(ulong address, Span<byte> buffer)
        => Virtual.Read(address, buffer);

    /// <inheritdoc/>
    public MemoryAccessResult TryWrite<T>(ulong address, T value)
        where T : unmanaged, IBinaryNumber<T>
        => Virtual.TryWrite(address, value);

    /// <inheritdoc/>
    public void Write<T>(ulong address, T value)
        where T : unmanaged, IBinaryNumber<T>
        => Virtual.Write(address, value);

    /// <inheritdoc/>
    public MemoryAccessResult TryWrite(ulong address, ReadOnlySpan<byte> buffer)
        => Virtual.TryWrite(address, buffer);

    /// <inheritdoc/>
    public void Write(ulong address, ReadOnlySpan<byte> buffer)
        => Virtual.Write(address, buffer);

    /// <inheritdoc/>
    public Stream AsStream() => Virtual.AsStream();
}
