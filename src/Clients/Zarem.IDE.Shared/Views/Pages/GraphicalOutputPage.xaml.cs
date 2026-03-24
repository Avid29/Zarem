// Avishai Dernis 2026

using Microsoft.UI.Xaml.Controls;
using Zarem.IDE.Services;
using Zarem.IDE.ViewModels.Pages;

namespace Zarem.IDE.Views.Pages;

public sealed partial class GraphicalOutputPage : UserControl
{
    public GraphicalOutputPage()
    {
        this.InitializeComponent();

        ViewModel = Service.Get<GraphicalOutputPageViewModel>();
    }

    private GraphicalOutputPageViewModel ViewModel { get; }
}
