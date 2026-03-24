// Avishai Dernis 2026

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Runtime.InteropServices;
using Windows.Graphics.DirectX;
using Zarem.Emulator.Machine.Devices.Interfaces;
using Zarem.IDE.Views.Pages;

namespace Zarem.IDE.Controls;

public sealed partial class GraphicsViewer : UserControl
{
    private CanvasBitmap? _canvasBitmap; // Use persistent bitmap to avoid re-allocation

    public static readonly DependencyProperty DeviceProperty =
        DependencyProperty.Register(nameof(Device), typeof(IGraphicsDevice), typeof(GraphicalOutputPage), new(null));

    public GraphicsViewer()
    {
        this.InitializeComponent();
    }

    public IGraphicsDevice? Device
    {
        get => (IGraphicsDevice?)GetValue(DeviceProperty);
        set => SetValue(DeviceProperty, value);
    }

    private unsafe void OnCanvasDraw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        if (Device is null)
            return;

        var buffer = Device.GetPixelBuffer();

        // Initialize or Re-initialize the bitmap if the resolution changed
        if (_canvasBitmap == null ||
            _canvasBitmap.SizeInPixels.Width != (uint)buffer.Width ||
            _canvasBitmap.SizeInPixels.Height != (uint)buffer.Height)
        {
            _canvasBitmap = CanvasBitmap.CreateFromBytes(
                sender,
                new byte[buffer.Width * buffer.Height * 4], // Initial empty buffer
                buffer.Width,
                buffer.Height,
                DirectXPixelFormat.B8G8R8X8UIntNormalized);
        }

        if (buffer.TryGetSpan(out var span))
        {
            // Optimized Data Transfer
            fixed (uint* ptr = span)
            {
                // SetPixelBytes can take a pointer or a byte array.
                // Converting Span to a temporary byte-view without copying:
                var byteSpan = MemoryMarshal.AsBytes(span);
                _canvasBitmap.SetPixelBytes(byteSpan.ToArray());
            }

            args.DrawingSession.DrawImage(_canvasBitmap);
        }
        else
        {
            // Fallback: If TryGetSpan fails (e.g., non-contiguous memory), 
            // we copy row-by-row using the Span2D structure.
            byte[] staging = new byte[buffer.Width * buffer.Height * 4];
            var stagingSpan = staging.AsSpan();

            for (int y = 0; y < buffer.Height; y++)
            {
                var row = MemoryMarshal.AsBytes(buffer.GetRowSpan(y));
                row.CopyTo(stagingSpan[(y * buffer.Width * 4)..]);
            }
            _canvasBitmap.SetPixelBytes(staging);

            args.DrawingSession.DrawImage(_canvasBitmap);
        }
    }
}
