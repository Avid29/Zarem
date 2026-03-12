// Avishai Dernis 2026

using System;
using Zarem.Emulator.Machine.Interfaces;

namespace Zarem.Emulator.Machine;

/// <summary>
/// A class for a system handling physical and virtual memory in an <see cref="IComputer"/>.
/// </summary>
public class MemorySystem : IMemorySystem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemorySystem"/> class.
    /// </summary>
    public MemorySystem(IMemoryAccessor physical, IAddressTranslator translator)
    {
        Physical = physical;
        Virtual = new VirtualMemorySystem(physical, translator);
    }

    /// <inheritdoc/>
    public IMemoryAccessor Physical { get; }

    /// <inheritdoc/>
    public IVirtualMemoryAccessor Virtual { get; }

    /// <inheritdoc/>
    public T Read<T>(ulong address) where T : unmanaged
        => Virtual.Read<T>(address);

    /// <inheritdoc/>
    public void Read(ulong address, Span<byte> buffer)
        => Virtual.Read(address, buffer);

    /// <inheritdoc/>
    public void Write<T>(ulong address, T value) where T : unmanaged
        => Virtual.Write(address, value);

    /// <inheritdoc/>
    public void Write(ulong address, ReadOnlySpan<byte> buffer)
        => Virtual.Write(address, buffer);
}
