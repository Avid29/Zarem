// Avishai Dernis 2026

using System;
using System.IO;
using Zarem.Emulator.Machine.Interfaces;

namespace Zarem.Emulator.Helpers;

/// <summary>
/// A <see cref="Stream"/> wrapping an <see cref="IMemoryAccessor"/>.
/// </summary>
public class BusStream : Stream
{
    private readonly IMemoryAccessor _bus;

    /// <summary>
    /// Initializes a new instance of the <see cref="BusStream"/> class.
    /// </summary>
    /// <param name="bus"></param>
    public BusStream(IMemoryAccessor bus)
    {
        _bus = bus;
    }

    /// <inheritdoc/>
    public override bool CanRead => true;

    /// <inheritdoc/>
    public override bool CanWrite => true;

    /// <inheritdoc/>
    public override bool CanSeek => true;

    /// <inheritdoc/>
    public override long Length => long.MaxValue; // The address space is the limit

    /// <inheritdoc/>
    public override long Position { get; set; }

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Write one byte at a time to the bus
            _bus.Write((ulong)Position++, buffer[offset + i]);
        }
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        int read = 0;
        for (int i = 0; i < count; i++)
        {
            buffer[offset + i] = _bus.Read<byte>((ulong)Position++);
            read++;
        }
        return read;
    }

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin) => origin switch
    {
        SeekOrigin.Begin => Position = offset,
        SeekOrigin.Current => Position += offset,
        SeekOrigin.End => throw new NotSupportedException("Bus has no defined end."),
        _ => Position
    };

    /// <inheritdoc/>
    public override void Flush()
    {
        // Nothing to be done
    }

    /// <inheritdoc/>
    public override void SetLength(long value) => throw new NotSupportedException();
}
