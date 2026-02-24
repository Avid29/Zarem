// Avishai Dernis 2026

using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using System.Numerics;
using Windows.UI;

namespace Zarem.WinUI.Controls;

/// <summary>
/// A control for a running indication.
/// </summary>
public sealed partial class RunningIndicator : UserControl
{
    private readonly Color _color1;
    private readonly Color _color2;

    public RunningIndicator()
    {
        this.InitializeComponent();


        _color1 = Color.FromArgb(255, 0x03, 0x4A, 0xC9);
        _color2 = Color.FromArgb(255, 0x58, 0x38, 0xFA);
    }

    private void Create2DGradient()
    {
        // Grab the compositor from that visual
        var canvasVisual = ElementCompositionPreview.GetElementVisual(GradientCanvas);
        var compositor = canvasVisual.Compositor;

        // Create the Color Gradient (Horizontal)
        var colorBrush = compositor.CreateLinearGradientBrush();
        colorBrush.StartPoint = new Vector2(0, 0.5f);
        colorBrush.EndPoint = new Vector2(1, 0.5f);
        colorBrush.ColorStops.Add(compositor.CreateColorGradientStop(0.0f, _color1));
        colorBrush.ColorStops.Add(compositor.CreateColorGradientStop(0.5f, _color2));
        colorBrush.ColorStops.Add(compositor.CreateColorGradientStop(1.0f, _color1));
        colorBrush.ExtendMode = CompositionGradientExtendMode.Wrap;

        // Create the Mask (Vertical Opacity)
        var maskBrush = compositor.CreateLinearGradientBrush();
        maskBrush.StartPoint = new Vector2(0.5f, 0);
        maskBrush.EndPoint = new Vector2(0.5f, 1);
        maskBrush.ColorStops.Add(compositor.CreateColorGradientStop(0.0f, Colors.Transparent)); // Opaque
        maskBrush.ColorStops.Add(compositor.CreateColorGradientStop(1.0f, Colors.Black)); // Clear

        // Combine them using a MaskBrush
        var finalBrush = compositor.CreateMaskBrush();
        finalBrush.Source = colorBrush;
        finalBrush.Mask = maskBrush;

        // Apply to a SpriteVisual
        var sprite = compositor.CreateSpriteVisual();
        sprite.Size = new Vector2((float)GradientCanvas.ActualWidth, (float)GradientCanvas.ActualHeight);
        sprite.Brush = finalBrush;

        ElementCompositionPreview.SetElementChildVisual(GradientCanvas, sprite);
    }

    private void GradientCanvas_Loaded(object sender, RoutedEventArgs e)
    {
        Create2DGradient();
    }
}
