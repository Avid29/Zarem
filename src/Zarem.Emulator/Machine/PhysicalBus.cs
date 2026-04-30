// Avishai Dernis 2026

using System;
using System.Buffers.Binary;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Zarem.Emulator.Devices.Interfaces;
using Zarem.Emulator.Helpers;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Models.Enums;

namespace Zarem.Emulator.Machine;

/// <summary>
/// Handles the operations of a physical bus in an emulated computer.
/// </summary>
public unsafe class PhysicalBus : IMemoryAccessor
{
    private readonly MemoryMapper _mapper;
    private readonly bool _endianMismatch;

    /// <summary>
    /// An event invoked when an address is written to.
    /// </summary>
    public event EventHandler<ulong>? AddressWritten;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhysicalBus"/> class.
    /// </summary>
    public PhysicalBus(MemoryMapper mapper, Endianness endianness)
    {
        _mapper = mapper;
        _endianMismatch = BitConverter.IsLittleEndian != (endianness == Endianness.Little);
    }

    /// <summary>
    /// Gets or sets the endianness of the bus.
    /// </summary>
    public Endianness Endianness { get; }

    /// <inheritdoc/>
    public T Read<T>(ulong address)
        where T : unmanaged, IBinaryNumber<T>
    {
        CheckAlignment<T>(address);

        var device = _mapper.Resolve(address, out var baseAddress);
        ulong offset = address - baseAddress;

        if (device is IBusDeviceDirect memDevice)
        {
            byte* ptr = memDevice.GetPointer(offset);
            T value = Unsafe.Read<T>(ptr);

            return _endianMismatch
                ? ReverseEndianness(value)
                : value;
        }

        // Fallback: MMIO/Hardware registers
        return ReadSlow<T>(device, offset);
    }

    /// <inheritdoc/>
    public void Write<T>(ulong address, T value)
        where T : unmanaged, IBinaryNumber<T>
    {
        CheckAlignment<T>(address);

        var device = _mapper.Resolve(address, out var baseAddress);
        ulong offset = address - baseAddress;

        if (device is IBusDeviceDirect memDevice)
        {
            byte* ptr = memDevice.GetPointer(offset);

            // Handle endianness swap before writing to raw memory
            if (_endianMismatch)
            {
                value = ReverseEndianness(value);
            }

            Unsafe.Write(ptr, value);
        }
        else
        {
            // Fallback: MMIO/Hardware registers
            WriteSlow(device, offset, value);
        }

        // Invoke the address written event
        AddressWritten?.Invoke(this, address);
    }

    /// <inheritdoc/>
    public Stream AsStream() => new BusStream(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CheckAlignment<T>(ulong address)
        where T : unmanaged, IBinaryNumber<T>
    {
        if ((address & (ulong)(sizeof(T) - 1)) != 0)
            throw new Exception($"Unaligned access at 0x{address:X16} for size {sizeof(T)}");
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T ReadEndianness<T>(ReadOnlySpan<byte> buffer)
        where T : unmanaged, IBinaryNumber<T>
    {
        T value = MemoryMarshal.Read<T>(buffer);

        // If the host endianness doesn't match the emulation endianness, swap it.
            return _endianMismatch
                ? ReverseEndianness(value)
                : value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteEndianness<T>(Span<byte> buffer, T value)
        where T : unmanaged, IBinaryNumber<T>
    {
        // If host matches target, just write raw bytes
        if (!_endianMismatch)
        {
            MemoryMarshal.Write(buffer, in value);
            return;
        }

        // No match. Change endianness before writing
        // The CLR optimizes this into a single path based on the caller's 'T'
        if (sizeof(T) == 1)
        {
            buffer[0] = Unsafe.As<T, byte>(ref value);
        }
        else if (sizeof(T) == 2)
        {
            ushort val = Unsafe.As<T, ushort>(ref value);
            if (BitConverter.IsLittleEndian)
                BinaryPrimitives.WriteUInt16BigEndian(buffer, val);
            else
                BinaryPrimitives.WriteUInt16LittleEndian(buffer, val);
        }
        else if (sizeof(T) == 4)
        {
            uint val = Unsafe.As<T, uint>(ref value);
            if (BitConverter.IsLittleEndian)
                BinaryPrimitives.WriteUInt32BigEndian(buffer, val);
            else
                BinaryPrimitives.WriteUInt32LittleEndian(buffer, val);
        }
        else if (sizeof(T) == 8)
        {
            ulong val = Unsafe.As<T, ulong>(ref value);
            if (BitConverter.IsLittleEndian)
                BinaryPrimitives.WriteUInt64BigEndian(buffer, val);
            else
                BinaryPrimitives.WriteUInt64LittleEndian(buffer, val);
        }
        else
        {
            throw new NotSupportedException();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T ReverseEndianness<T>(T value)
        where T : unmanaged, IBinaryNumber<T>
    {
        // sizeof(T) is a JIT constant. No branches in the final assembly.
        if (sizeof(T) == 1)
            return value;

        if (sizeof(T) == 2)
        {
            // Reinterprets the bytes of T as a ushort without boxing or conversion logic.
            ushort val = Unsafe.As<T, ushort>(ref value);
            ushort swapped = BinaryPrimitives.ReverseEndianness(val);
            return Unsafe.As<ushort, T>(ref swapped);
        }

        if (sizeof(T) == 4)
        {
            uint val = Unsafe.As<T, uint>(ref value);
            uint swapped = BinaryPrimitives.ReverseEndianness(val);
            return Unsafe.As<uint, T>(ref swapped);
        }

        if (sizeof(T) == 8)
        {
            ulong val = Unsafe.As<T, ulong>(ref value);
            ulong swapped = BinaryPrimitives.ReverseEndianness(val);
            return Unsafe.As<ulong, T>(ref swapped);
        }

        throw new NotSupportedException($"Size {sizeof(T)} not supported for endianness swap.");
    }

    /// <inheritdoc/>
    public void Read(ulong address, Span<byte> buffer)
    {
        var device = _mapper.Resolve(address, out var baseAddress);
        ulong offset = address - baseAddress;

        if (offset + (ulong)buffer.Length > device.BusRangeSize)
            throw new Exception("Cross-device multi-byte read is not supported.");

        device.Read(offset, buffer);
    }

    /// <inheritdoc/>
    public void Write(ulong address, ReadOnlySpan<byte> buffer)
    {
        var device = _mapper.Resolve(address, out var baseAddress);
        ulong offset = address - baseAddress;

        device.Write(offset, buffer);
        AddressWritten?.Invoke(this, address);
    }

    private T ReadSlow<T>(IBusDevice device, ulong offset) where T : unmanaged, IBinaryNumber<T>
    {
        int size = sizeof(T);
        // Use a fixed buffer on the stack to avoid span overhead in the slow path
        byte* buffer = stackalloc byte[size];
        var span = new Span<byte>(buffer, size);

        device.Read(offset, span);
        return ReadEndianness<T>(span);
    }

    private void WriteSlow<T>(IBusDevice device, ulong offset, T value)
        where T : unmanaged, IBinaryNumber<T>
    {
        int size = sizeof(T);
        byte* buffer = stackalloc byte[size];
        var span = new Span<byte>(buffer, size);

        WriteEndianness(span, value);

        device.Write(offset, span);
    }
}
