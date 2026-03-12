// Avishai Dernis 2026

using System;
using System.Buffers.Binary;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Zarem.Emulator.Machine.Interfaces;
using Zarem.Models.Enums;

namespace Zarem.Emulator.Machine;

/// <summary>
/// Handles the operations of a physical bus in an emulated computer.
/// </summary>
public class PhysicalBus : IMemoryAccessor
{
    private readonly MemoryMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhysicalBus"/> class.
    /// </summary>
    public PhysicalBus(MemoryMapper mapper)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// Gets or sets the endianness of the bus.
    /// </summary>
    public Endianness Endianness { get; set; } = Endianness.Big;

    /// <inheritdoc/>
    public T Read<T>(ulong address)
        where T : unmanaged
    {
        int size = Unsafe.SizeOf<T>();
        CheckAlignment(address, size);

        Span<byte> buffer = stackalloc byte[size];
        Read(address, buffer);

        return ReadEndianness<T>(buffer);
    }

    /// <inheritdoc/>
    public void Write<T>(ulong address, T value)
        where T : unmanaged
    {
        int size = Unsafe.SizeOf<T>();
        CheckAlignment(address, size);

        Span<byte> buffer = stackalloc byte[size];
        WriteEndianness(buffer, value);

        Write(address, buffer);
    }

    /// <inheritdoc/>
    public Stream AsStream()
    {
        throw new NotImplementedException();
    }

    private static void CheckAlignment(ulong address, int size)
    {
        if (address % (ulong)size != 0)
            throw new Exception($"Unaligned access at 0x{address:X16} for size {size}");
    }

    private T ReadEndianness<T>(ReadOnlySpan<byte> buffer) where T : unmanaged
    {
        // No endianness difference
        if (BitConverter.IsLittleEndian == Endianness is Endianness.Little)
            return MemoryMarshal.Read<T>(buffer);

        return Endianness switch
        {
            Endianness.Big => typeof(T) switch
            {
                Type t when t == typeof(ulong) => (T)(object)BinaryPrimitives.ReadUInt64BigEndian(buffer),
                Type t when t == typeof(long) => (T)(object)BinaryPrimitives.ReadInt64BigEndian(buffer),
                Type t when t == typeof(uint) => (T)(object)BinaryPrimitives.ReadUInt32BigEndian(buffer),
                Type t when t == typeof(int) => (T)(object)BinaryPrimitives.ReadInt32BigEndian(buffer),
                Type t when t == typeof(ushort) => (T)(object)BinaryPrimitives.ReadUInt16BigEndian(buffer),
                Type t when t == typeof(short) => (T)(object)BinaryPrimitives.ReadInt16BigEndian(buffer),
                _ => MemoryMarshal.Read<T>(buffer),
            },
            Endianness.Little or _ => typeof(T) switch
            {
                Type t when t == typeof(ulong) => (T)(object)BinaryPrimitives.ReadUInt64LittleEndian(buffer),
                Type t when t == typeof(long) => (T)(object)BinaryPrimitives.ReadInt64LittleEndian(buffer),
                Type t when t == typeof(uint) => (T)(object)BinaryPrimitives.ReadUInt32LittleEndian(buffer),
                Type t when t == typeof(int) => (T)(object)BinaryPrimitives.ReadInt32LittleEndian(buffer),
                Type t when t == typeof(ushort) => (T)(object)BinaryPrimitives.ReadUInt16LittleEndian(buffer),
                Type t when t == typeof(short) => (T)(object)BinaryPrimitives.ReadInt16LittleEndian(buffer),
                _ => MemoryMarshal.Read<T>(buffer),
            },
        };
    }

    private void WriteEndianness<T>(Span<byte> buffer, T value) where T : unmanaged
    {
        // No endianness difference
        if (BitConverter.IsLittleEndian == Endianness is Endianness.Little)
        {
            MemoryMarshal.Write(buffer, in value);
            return;
        }

        switch (Endianness)
        {
            case Endianness.Big:
                switch (value)
                {
                    case ulong u64:
                        BinaryPrimitives.WriteUInt64BigEndian(buffer, u64);
                        break;
                    case uint u32:
                        BinaryPrimitives.WriteUInt32BigEndian(buffer, u32);
                        break;
                    case ushort u16:
                        BinaryPrimitives.WriteUInt16BigEndian(buffer, u16);
                        break;
                    default:
                        MemoryMarshal.Write(buffer, in value);
                        break;
                }
                break;
            case Endianness.Little:
                switch (value)
                {
                    case ulong u64:
                        BinaryPrimitives.WriteUInt64LittleEndian(buffer, u64);
                        break;
                    case uint u32:
                        BinaryPrimitives.WriteUInt32LittleEndian(buffer, u32);
                        break;
                    case ushort u16:
                        BinaryPrimitives.WriteUInt16LittleEndian(buffer, u16);
                        break;
                    default:
                        MemoryMarshal.Write(buffer, in value);
                        break;
                }
                break;
        };
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
}
