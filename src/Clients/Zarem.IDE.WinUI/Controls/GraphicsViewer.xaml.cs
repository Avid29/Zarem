// Avishai Dernis 2026

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Runtime.InteropServices;
using Windows.Graphics.DirectX;
using Zarem.Emulator.Machine.Devices.Interfaces;
using Zarem.IDE.Views.Pages;

namespace Zarem.IDE.Controls;

public sealed partial class GraphicsViewer : UserControl
{
    private CanvasBitmap? _canvasBitmap;    // Use persistent bitmap to avoid re-allocation
    private byte[]? _stagingBuffer;         // Reuse this array to stop GC pressure
    private IGraphicsDevice? _cachedDevice; // Avoid DP lookup in hot path

    public static readonly DependencyProperty DeviceProperty =
        DependencyProperty.Register(nameof(Device), typeof(IGraphicsDevice), typeof(GraphicalOutputPage), new(null, OnDevicePropertyChanged));

    public GraphicsViewer()
    {
        this.InitializeComponent();

        CompositionTarget.Rendering += CompositionTarget_Rendering;
    }

    private void CompositionTarget_Rendering(object? sender, object e)
    {
        if (Device?.IsDirty is true)
        {
            DisplayCanvas.Invalidate();
        }
    }

    public IGraphicsDevice? Device
    {
        get => (IGraphicsDevice?)GetValue(DeviceProperty);
        set => SetValue(DeviceProperty, value);
    }

    private void OnCanvasDraw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        // Use a cached local reference to avoid DependencyProperty overhead
        var device = _cachedDevice;
        if (device is null || !device.IsDirty)
        {
            if (_canvasBitmap is not null)
            {
                args.DrawingSession.DrawImage(_canvasBitmap);
            }

            return;
        }

        var buffer = device.GetPixelBuffer();
        int totalBytes = buffer.Width * buffer.Height * 4;

        // Ensure staging buffer and bitmap exist and match resolution
        if (_stagingBuffer == null || _stagingBuffer.Length != totalBytes)
        {
            _stagingBuffer = new byte[totalBytes];
            _canvasBitmap = CanvasBitmap.CreateFromBytes(sender, _stagingBuffer, buffer.Width, buffer.Height, DirectXPixelFormat.B8G8R8X8UIntNormalized);
            sender.Width = buffer.Width;
            sender.Height = buffer.Height;
        }

        // Copy data into the staging buffer (Persistent, so no allocations!)
        if (buffer.TryGetSpan(out var span))
        {
            MemoryMarshal.AsBytes(span).CopyTo(_stagingBuffer);
        }
        else
        {
            for (int y = 0; y < buffer.Height; y++)
            {
                var row = MemoryMarshal.AsBytes(buffer.GetRowSpan(y));
                row.CopyTo(_stagingBuffer.AsSpan(y * buffer.Width * 4));
            }
        }

        // Update the GPU texture
        _canvasBitmap!.SetPixelBytes(_stagingBuffer);
        args.DrawingSession.DrawImage(_canvasBitmap);

        device.IsDirty = false;
    }

    private static void OnDevicePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        if (obj is not GraphicsViewer viewer)
            return;

        if (args.Property != DeviceProperty)
            return;

        if (args.NewValue is IGraphicsDevice newDevice)
            viewer._cachedDevice = newDevice;
    }
}
