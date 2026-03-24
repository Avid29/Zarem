// Avishai Dernis 2026

using System;
using System.Buffers.Binary;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Zarem.Emulator.Helpers;
using Zarem.Emulator.Machine.Devices;
using Zarem.Emulator.Machine.Devices.Interfaces;
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
    /// Initializes a new instance of the <see cref="PhysicalBus"/> class.
    /// </summary>
    public PhysicalBus(MemoryMapper mapper, Endianness endianness = Endianness.Big)
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

        if (device is RamDevice memDevice)
        {
            byte* ptr = memDevice.GetPointer(offset);
            T value = Unsafe.Read<T>(ptr);

            return _endianMismatch
                ? ReverseEndianness(value)
                : value;
        }

        return ReadSlow<T>(device, offset);
    }

    /// <inheritdoc/>
    public void Write<T>(ulong address, T value)
        where T : unmanaged, IBinaryNumber<T>
    {
        CheckAlignment<T>(address);

        Span<byte> buffer = stackalloc byte[sizeof(T)];
        WriteEndianness(buffer, value);

        Write(address, buffer);
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

        // If the system endianness doesn't match the target MIPS endianness, swap it.
        if (_endianMismatch)
            return ReverseEndianness(value);

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T ReverseEndianness<T>(T value)
        where T : unmanaged, IBinaryNumber<T>
    {
        // These 'if' checks on sizeof are resolved at COMPILE TIME by the JIT.
        // The code for the "wrong" sizes is completely deleted from the final machine code.
        if (sizeof(T) == 1) return value;
        if (sizeof(T) == 2) return (T)(object)BinaryPrimitives.ReverseEndianness((ushort)(object)value);
        if (sizeof(T) == 4) return (T)(object)BinaryPrimitives.ReverseEndianness((uint)(object)value);
        if (sizeof(T) == 8) return (T)(object)BinaryPrimitives.ReverseEndianness((ulong)(object)value);

        throw new NotSupportedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteEndianness<T>(Span<byte> buffer, T value)
        where T : unmanaged, IBinaryNumber<T>
    {
        // If host matches target, just write raw bytes
        if (BitConverter.IsLittleEndian == (Endianness == Endianness.Little))
        {
            MemoryMarshal.Write(buffer, in value);
            return;
        }

        // No match. Change endianness before writing
        // The JIT optimizes this into a single path based on the caller's 'T'
        if (sizeof(T) == 1)
        {
            buffer[0] = Unsafe.As<T, byte>(ref value);
        }
        else if (sizeof(T) == 2)
        {
            ushort val = Unsafe.As<T, ushort>(ref value);
            if (Endianness == Endianness.Big)
                BinaryPrimitives.WriteUInt16BigEndian(buffer, val);
            else
                BinaryPrimitives.WriteUInt16LittleEndian(buffer, val);
        }
        else if (sizeof(T) == 4)
        {
            uint val = Unsafe.As<T, uint>(ref value);
            // Note: Using the 'Opposite' primitive is often faster than Manual Reverse + Write
            if (Endianness == Endianness.Big)
                BinaryPrimitives.WriteUInt32BigEndian(buffer, val);
            else
                BinaryPrimitives.WriteUInt32LittleEndian(buffer, val);
        }
        else if (sizeof(T) == 8)
        {
            ulong val = Unsafe.As<T, ulong>(ref value);
            if (Endianness == Endianness.Big)
                BinaryPrimitives.WriteUInt64BigEndian(buffer, val);
            else
                BinaryPrimitives.WriteUInt64LittleEndian(buffer, val);
        }
        else
        {
            throw new NotSupportedException();
        }
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
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private T ReadSlow<T>(IBusDevice device, ulong offset) where T : unmanaged, IBinaryNumber<T>
    {
        int size = sizeof(T);
        // Use a fixed buffer on the stack to avoid span overhead in the slow path
        byte* buffer = stackalloc byte[size];
        var span = new Span<byte>(buffer, size);

        device.Read(offset, span);
        return ReadEndianness<T>(span);
    }
}
