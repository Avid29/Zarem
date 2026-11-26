// Avishai Dernis 2026

using Microsoft.UI.Xaml.Controls;
using Zarem.SAE.ViewModels;

namespace Zarem.SAE.Views.Shell;

public sealed partial class OverlayView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OverlayView"/> class.
    /// </summary>
    public OverlayView()
    {
        InitializeComponent();
    }

    private OverlayViewModel ViewModel => (OverlayViewModel)DataContext;
}
