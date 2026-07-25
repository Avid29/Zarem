// Avishai Dernis 2026

using System;
using System.IO;
using System.Numerics;
using Zarem.Emulator.Helpers;
using Zarem.Emulator.Models.Enums;

namespace Zarem.Emulator.Machine.Memory;

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
    public ulong Translate(ulong virtualAddress) => _translator.Translate(virtualAddress);

    /// <inheritdoc/>
    public MemoryAccessResult TryTranslate(ulong virtualAddress, out ulong address) => _translator.TryTranslate(virtualAddress, out address);

    /// <inheritdoc/>
    public MemoryAccessResult TryRead<T>(ulong address, out T value)
        where T : unmanaged, IBinaryNumber<T>
    {
        value = default;

        var result = _translator.TryTranslate(address, out var pAddress);
        if (result is not MemoryAccessResult.Success)
            return result;

        return _physical.TryRead(pAddress, out value);
    }

    /// <inheritdoc/>
    public T Read<T>(ulong address)
        where T : unmanaged, IBinaryNumber<T>
    {
        ulong pAddress = _translator.Translate(address);
        return _physical.Read<T>(pAddress);
    }

    /// <inheritdoc/>
    public MemoryAccessResult TryRead(ulong address, Span<byte> buffer)
    {
        var result = _translator.TryTranslate(address, out var pAddress);
        if (result is not MemoryAccessResult.Success)
            return result;

        return _physical.TryRead(pAddress, buffer);
    }

    /// <inheritdoc/>
    public void Read(ulong address, Span<byte> buffer)
    {
        ulong pAddress = _translator.Translate(address);
        _physical.Read(pAddress, buffer);
    }

    /// <inheritdoc/>
    public MemoryAccessResult TryWrite<T>(ulong address, T value)
        where T : unmanaged, IBinaryNumber<T>
    {
        var result = _translator.TryTranslate(address, out var pAddress);
        if (result is not MemoryAccessResult.Success)
            return result;

        return _physical.TryWrite(pAddress, value);
    }

    /// <inheritdoc/>
    public void Write<T>(ulong address, T value)
        where T : unmanaged, IBinaryNumber<T>
    {
        ulong pAddress = _translator.Translate(address);
        _physical.Write(pAddress, value);
    }

    public MemoryAccessResult TryWrite(ulong address, ReadOnlySpan<byte> buffer)
    {
        var result = _translator.TryTranslate(address, out var pAddress);
        if (result is not MemoryAccessResult.Success)
            return result;

        return _physical.TryWrite(pAddress, buffer);
    }

    /// <inheritdoc/>
    public void Write(ulong address, ReadOnlySpan<byte> buffer)
    {
        ulong pAddress = _translator.Translate(address);
        _physical.Write(pAddress, buffer);
    }

    /// <inheritdoc/>
    public Stream AsStream() => new BusStream(this);
}
