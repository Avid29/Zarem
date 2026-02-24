// Avishai Dernis 2025

using CommunityToolkit.Mvvm.ComponentModel;

namespace Zarem.ViewModels.Pages.App.Settings;

/// <summary>
/// A base class for settings sub-page view models.
/// </summary>
public abstract class SettingsSubPageViewModel : ObservableObject
{
    /// <summary>
    /// Gets or sets the title of the settings sub-page.
    /// </summary>
    public abstract string Title { get; }
}
