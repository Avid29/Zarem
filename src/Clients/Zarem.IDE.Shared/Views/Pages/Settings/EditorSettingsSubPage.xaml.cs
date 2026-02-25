// Avishai Dernis 2025

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Zarem.IDE.Messages;
using Zarem.IDE.Models.EditorConfig.ColorScheme;
using Zarem.IDE.Services;
using Zarem.IDE.Services.Settings.Enums;
using Zarem.IDE.ViewModels.Pages.Settings;

namespace Zarem.IDE.Views.Pages.Settings;

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
