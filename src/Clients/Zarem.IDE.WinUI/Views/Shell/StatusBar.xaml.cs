// Avishai Dernis 2025

using Microsoft.UI.Xaml.Controls;
using Zarem.Services;
using Zarem.ViewModels;

namespace Zarem.WinUI.Views.Shell;

/// <summary>
/// The status bar.
/// </summary>
public sealed partial class StatusBar : UserControl
{
    public StatusBar()
    {
        this.InitializeComponent();

        this.DataContext = Service.Get<StatusViewModel>();
    }

    private StatusViewModel ViewModel => (StatusViewModel)DataContext;
}
