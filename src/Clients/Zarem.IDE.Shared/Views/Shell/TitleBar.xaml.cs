// Avishai Dernis 2024

using CommunityToolkit.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Graphics;
using Windows.System;
using Zarem.IDE.ViewModels;

namespace Zarem.IDE.Views.Shell;

/// <summary>
/// The title bar contents.
/// </summary>
public sealed partial class TitleBar : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TitleBar"/> class.
    /// </summary>
    public TitleBar()
    {
        this.InitializeComponent();

        Loaded += TitleBar_Loaded;
        SizeChanged += TitleBar_SizeChanged;
    }

    private void TitleBar_SizeChanged(object sender, SizeChangedEventArgs e) => AdjustDragRegion();

    private WindowViewModel ViewModel => (WindowViewModel)DataContext;

    private void TitleBar_Loaded(object sender, RoutedEventArgs e)
    {
        ExtendIntoTitleBar();
    }

    private void ExtendIntoTitleBar()
    {
        var window = App.Current.Window;
        Guard.IsNotNull(window);

        window.ExtendsContentIntoTitleBar = true;
        AdjustDragRegion();
    }

    private void AdjustDragRegion()
    {
        var window = App.Current.Window;
        Guard.IsNotNull(window);

        var rect = new RectInt32
        {
            X = 0,
            Y = 0,
            Width = (int)window.Bounds.Width,
            Height = (int)ActualHeight,
        };

        window.AppWindow.TitleBar.SetDragRectangles([rect]);
    }

    public static bool IsNotNull(object? obj) => obj is not null;

    private async void WikiMFI_Click(object sender, RoutedEventArgs e)
        => await Launcher.LaunchUriAsync(new("https://github.com/Avid29/Zarem/wiki"));
}
