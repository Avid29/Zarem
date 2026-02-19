// Avishai Dernis 2025

using Microsoft.UI.Xaml.Controls;
using Zarem.ViewModels.Pages.App.Settings;

namespace Zarem.WinUI.Views.Pages.App.SettingsSubPages;

/// <summary>
/// The app settings subpage.
/// </summary>
public sealed partial class AppSettingsSubPage : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppSettingsSubPage"/> class.
    /// </summary>
    public AppSettingsSubPage()
    {
        this.InitializeComponent();
    }

    public AppSettingsViewModel? ViewModel { get; set; }

}
