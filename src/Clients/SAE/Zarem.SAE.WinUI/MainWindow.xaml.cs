// Avishai Dernis 2026

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Zarem.SAE.ViewModels;

namespace Zarem.SAE;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ViewModel = App.Current.Services.GetRequiredService<MainViewModel>();
    }

    private MainViewModel ViewModel { get; }

    private async void RootGrid_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.InitializeEmulatorsAsync();
    }
}
