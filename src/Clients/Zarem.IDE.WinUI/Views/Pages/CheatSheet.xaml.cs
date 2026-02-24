// Avishai Dernis 2025

using Microsoft.UI.Xaml.Controls;
using Zarem.ViewModels.Pages;
using System.Linq;

namespace Zarem.WinUI.Views.Pages;

public sealed partial class CheatSheet : UserControl
{
    public CheatSheet()
    {
        InitializeComponent();
    }

    public CheatSheetViewModel? ViewModel { get; set; }

    private void UserControl_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ViewModel is null)
            return;

        // This is stupid, but whatever for now
        RegisterEncodingTable.CellData = ViewModel.GPRegisters.Select(x => (object)x).ToArray();
    }
}
