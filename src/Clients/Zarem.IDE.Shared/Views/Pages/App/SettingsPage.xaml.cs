// Avishai Dernis 2025

using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Zarem.Services;
using Zarem.ViewModels.Pages.App;
using System.Globalization;

namespace Zarem.IDE.Views.Pages.App;

/// <summary>
/// The settings page.
/// </summary>
public sealed partial class SettingsPage : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsPage"/> class.
    /// </summary>
    public SettingsPage()
    {
        this.InitializeComponent();
    }

    public SettingsPageViewModel? ViewModel { get; set; }

    public static string? LanguageDisplayName(string? code)
    {
        if (code is null)
            return null;

        // "system" and "app" are sentinel values since null and empty cannot be used in a ComboBox
        var localization = Service.Get<ILocalizationService>();
        switch (code)
        {
            case "system":
                return localization["/Settings/UseSystemLanguage"];
            case "app":
                return localization["/Settings/UseAppLanguage"];
        }

        var target = new CultureInfo(code);
        var nativeName = target.NativeName;
        var displayName = target.DisplayName;

        // Avoid displaying redundant info
        if (nativeName == displayName)
            return nativeName;

        return $"{nativeName} - {displayName}";
    }
}
