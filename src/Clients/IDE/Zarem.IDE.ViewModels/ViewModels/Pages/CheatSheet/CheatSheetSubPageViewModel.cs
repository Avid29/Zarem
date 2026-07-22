// Avishai Dernis 2026

using CommunityToolkit.Mvvm.ComponentModel;
using Zarem.CheatSheet;

namespace Zarem.IDE.ViewModels.Pages.CheatSheet;

/// <summary>
/// A base class for cheatsheet sub-page view models
/// </summary>
public abstract class CheatSheetSubPageViewModel : ObservableObject
{
    /// <summary>
    /// Gets or sets the title of the cheatsheet sub-page.
    /// </summary>
    public abstract string Title { get; }

    /// <summary>
    /// Gets or sets the loaded <see cref="CheatSheetPage"/>.
    /// </summary>
    public CheatSheetPage? CheatSheetPage
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
                Refresh();
        }
    }

    private protected virtual void Refresh()
    {
    }
}
