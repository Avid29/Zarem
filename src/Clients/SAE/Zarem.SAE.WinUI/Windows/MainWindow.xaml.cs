// Avishai Dernis 2026

using Microsoft.UI.Xaml;

namespace Zarem.SAE.Windows;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : ZaremWindow
{
    public MainWindow()
    {
        InitializeComponent();
        SetupWindowStyle();
    }
}
