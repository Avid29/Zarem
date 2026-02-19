// Avishai Dernis 2025

namespace Zarem.WinUI.Windows;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class PanelWindow : ZaremWindow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PanelWindow"/> class.
    /// </summary>
    public PanelWindow()
    {
        InitializeComponent();
        SetupWindowStyle();

        ExtendsContentIntoTitleBar = true;
    }
}
