// Avishai Dernis 2025

using Microsoft.UI.Xaml.Controls;
using Zarem.IDE.ViewModels.Pages.App.Settings;

namespace Zarem.IDE.Views.Pages.App.SettingsSubPages;

/// <summary>
/// The app settings subpage
/// </summary>
public sealed partial class AssemblerSettingsSubPage : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblerSettingsSubPage"/> class.
    /// </summary>
    public AssemblerSettingsSubPage()
    {
        this.InitializeComponent();
    }

    public AssemblerSettingsViewModel? ViewModel { get; set; }

}
