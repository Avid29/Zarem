// Avishai Dernis 2024

namespace Zarem.WinUI.Windows;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : ZaremWindow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        SetupWindowStyle();

        // Open the welcome page on startup
        ViewModel.OpenWelcomeCommand.Execute(null);
    }
}
