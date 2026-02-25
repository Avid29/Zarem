// Avishai Dernis 2026

using Microsoft.UI.Xaml.Controls;
using Zarem.IDE.ViewModels.Pages.CheatSheet;

namespace Zarem.IDE.Views.Pages.CheatSheet;

public sealed partial class UsagePatternsSubPage : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UsagePatternsSubPage"/> class.
    /// </summary>
    public UsagePatternsSubPage()
    {
        this.InitializeComponent();
    }

    public UsagePatternsViewModel? ViewModel { get; set; }
}
