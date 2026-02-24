// Avishai Dernis 2025

using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Zarem.Messages;
using Zarem.Models.EditorConfig.ColorScheme;
using Zarem.Services;
using Zarem.Services.Settings.Enums;
using Zarem.ViewModels.Pages.App.Settings;

namespace Zarem.IDE.Views.Pages.App.SettingsSubPages;

/// <summary>
/// The app settings subpage
/// </summary>
public sealed partial class EditorSettingsSubPage : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EditorSettingsSubPage"/> class.
    /// </summary>
    public EditorSettingsSubPage()
    {
        this.InitializeComponent();

        Service.Get<IMessenger>().Register<EditorSettingsSubPage, SettingChangedMessage<Theme>>(this, (r, m) => SyntaxHighlighting.ReloadFromSettings());
        Service.Get<IMessenger>().Register<EditorSettingsSubPage, SettingChangedMessage<EditorColorScheme>>(this, (r, m) => SyntaxHighlighting.ReloadFromSettings());
    }

    public EditorSettingsViewModel? ViewModel { get; set; }

    public string DemoText = Service.Get<ILocalizationService>()["/Settings/EditorDemoText"];
}
