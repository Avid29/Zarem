// Avishai Dernis 2025

using Microsoft.UI.Xaml.Controls;
using Zarem.SAE.ViewModels.Pages.Settings;

namespace Zarem.SAE.Views.Pages.Settings;

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

    public SaeAppSettingsViewModel? ViewModel { get; set; }
}
