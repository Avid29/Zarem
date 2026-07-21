// Avishai Dernis 2026

using CommunityToolkit.HighPerformance;
using System;
using System.Runtime.InteropServices;
using Zarem.Emulator.Devices.Interfaces;

namespace Zarem.Emulator.Devices;

/// <summary>
/// The "Zarem Graphical Buffer Unit".
/// </summary>
/// <remarks>
/// Just a basic <see cref="IGraphicsDevice"/> to dump images.
/// </remarks>
public class ZaremGBU : IBusDevice, IGraphicsDevice
{
    private readonly uint[] _framebuffer;
    private readonly int _width;
    private readonly int _height;

    /// <inheritdoc/>
    public event EventHandler? Refresh;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZaremGBU"/> class.
    /// </summary>
    public ZaremGBU(int width = 640, int height = 480)
    {
        _width = width;
        _height = height;
        _framebuffer = new uint[width * height];
    }

    /// <inheritdoc/>
    public string Name => "Zarem Graphical Buffer Unit";

    /// <inheritdoc/>
    public ulong BusRangeSize => (ulong)(_width * _height * sizeof(uint));

    /// <inheritdoc/>
    public bool IsDirty
    {
        get;
        set
        {
            field = value;
            Refresh?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <inheritdoc/>
    public ReadOnlySpan2D<uint> GetPixelBuffer() => new(_framebuffer, _height, _width);

    /// <inheritdoc/>
    public void Read(ulong offset, Span<byte> destination)
    {
        if (offset + (ulong)destination.Length > BusRangeSize)
            return;

        var sourceSpan = MemoryMarshal.AsBytes(_framebuffer.AsSpan());
        sourceSpan.Slice((int)offset, destination.Length).CopyTo(destination);
    }

    /// <inheritdoc/>
    public void Write(ulong offset, ReadOnlySpan<byte> source)
    {
        var destSpan = MemoryMarshal.AsBytes(_framebuffer.AsSpan());
        source.CopyTo(destSpan[(int)offset..]);

        IsDirty = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
    }
}
