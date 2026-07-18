// Avishai Dernis 2026

using Microsoft.UI.Xaml.Controls;
using Zarem.IDE.ViewModels.Pages.CheatSheet;

namespace Zarem.IDE.Views.Pages.CheatSheet;

public sealed partial class EncodingPatternsSubPage : UserControl
{
    public EncodingPatternsSubPage()
    {
        this.InitializeComponent();
    }

    public EncodingPatternsViewModel? ViewModel { get; set; }
}
