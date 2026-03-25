// Avishai Dernis 2026

using System;
using System.IO;
using System.Numerics;
using Zarem.Emulator.Helpers;
using Zarem.Emulator.Machine.Interfaces;

namespace Zarem.Emulator.Machine;

internal class VirtualMemorySystem : IVirtualMemoryAccessor
{
    private readonly PhysicalBus _physical;
    private readonly IAddressTranslator _translator;

    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualMemorySystem"/> class.
    /// </summary>
    public VirtualMemorySystem(PhysicalBus physical, IAddressTranslator translator)
    {
        _physical = physical;
        _translator = translator;
    }

    /// <inheritdoc/>
    public T Read<T>(ulong address) where T : unmanaged, IBinaryNumber<T>
    {
        ulong pAddress = _translator.Translate(address);
        return _physical.Read<T>(pAddress);
    }

    public void Read(ulong address, Span<byte> buffer)
    {
        ulong pAddress = _translator.Translate(address);
        _physical.Read(pAddress, buffer);
    }

    /// <inheritdoc/>
    public ulong Translate(ulong virtualAddress) => _translator.Translate(virtualAddress);

    /// <inheritdoc/>
    public bool TryTranslate(ulong virtualAddress, out ulong address) => _translator.TryTranslate(virtualAddress, out address);

    /// <inheritdoc/>
    public void Write<T>(ulong address, T value) where T : unmanaged, IBinaryNumber<T>
    {
        ulong pAddress = _translator.Translate(address);
        _physical.Write(pAddress, value);
    }

    public void Write(ulong address, ReadOnlySpan<byte> buffer)
    {
        ulong pAddress = _translator.Translate(address);
        _physical.Write(pAddress, buffer);
    }

    /// <inheritdoc/>
    public Stream AsStream() => new BusStream(this);
}
