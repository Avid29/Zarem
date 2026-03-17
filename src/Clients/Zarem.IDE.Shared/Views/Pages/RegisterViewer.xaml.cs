// Avishai Dernis 2026

using Microsoft.UI.Xaml.Controls;
using Zarem.IDE.Services;
using Zarem.IDE.ViewModels.Pages;

namespace Zarem.IDE.Views.Pages;

public sealed partial class RegisterViewer : UserControl
{
    public RegisterViewer()
    {
        this.InitializeComponent();

        ViewModel = Service.Get<RegisterViewerViewModel>();
    }

    private RegisterViewerViewModel ViewModel { get; }
}
