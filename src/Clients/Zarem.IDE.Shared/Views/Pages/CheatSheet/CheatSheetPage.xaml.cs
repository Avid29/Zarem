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

    private void UserControl_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ViewModel is null)
            return;

        // This is stupid, but whatever for now
        //RegisterEncodingTable.CellData = ViewModel.GPRegisters.Select(x => (object)x).ToArray();
    }
}
