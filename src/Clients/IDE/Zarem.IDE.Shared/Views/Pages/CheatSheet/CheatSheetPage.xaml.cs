// Avishai Dernis 2025

using Microsoft.UI.Xaml.Controls;
using Zarem.IDE.ViewModels.Pages.CheatSheet;

namespace Zarem.IDE.Views.Pages.CheatSheet;

public sealed partial class CheatSheetPage : UserControl
{
    public CheatSheetPage()
    {
        InitializeComponent();
    }

    public CheatSheetViewModel? ViewModel { get; set; }
}
