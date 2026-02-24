// Avishai Dernis 2025

using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;

namespace Zarem.ViewModels.Pages.Abstract;

/// <summary>
/// A base class for page view models.
/// </summary>
public abstract class PageViewModel : ObservableRecipient
{
    /// <summary>
    /// Gets or sets the title of the page.
    /// </summary>
    public abstract string Title { get; }

    /// <summary>
    /// Gets or sets if the page can be edited as text.
    /// </summary>
    public virtual bool CanTextEdit => false;

    /// <summary>
    /// Gets or sets if the page can be saved.
    /// </summary>
    public virtual bool CanSave => false;

    /// <summary>
    /// Gets or sets if the page contains unsaved content.
    /// </summary>
    public virtual bool IsDirty => false;

    /// <summary>
    /// Gets or sets if the page can be assembled.
    /// </summary>
    public virtual bool CanAssemble => false;

    /// <summary>
    /// Saves changes for an editable page.
    /// </summary>
    /// <exception cref="InvalidOperationException">Will be thrown if <see cref="CanSave"/> is false.</exception>
    public virtual async Task SaveAsync()
    {
        ThrowHelper.ThrowInvalidOperationException();
    }
}
