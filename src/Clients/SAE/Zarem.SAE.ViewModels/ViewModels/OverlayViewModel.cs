// Avishai Dernis 2026

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using Zarem.Services;
using Zarem.ViewModels.Pages.Abstract;

namespace Zarem.SAE.ViewModels;

/// <summary>
/// A view model for managing the overlay.
/// </summary>
public partial class OverlayViewModel : ObservableObject
{
    private readonly Stack<PageViewModel> _navStack;

    /// <summary>
    /// Initializes a new instance of the <see cref="OverlayViewModel"/> class.
    /// </summary>
    public OverlayViewModel()
    {
        _navStack = [];
    }

    /// <summary>
    /// Gets or sets the currently selected page.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPageOpen))]
    public partial PageViewModel? CurrentPage { get; set; }

    /// <summary>
    /// Gets a value indicating whether or not a page is open.
    /// </summary>
    public bool IsPageOpen => CurrentPage is not null;

    /// <summary>
    /// Opens a page as an overlay 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void OpenPage<T>()
        where T : PageViewModel
    {
        var page = Service.Get<T>();
        OpenPage(page);
    }

    /// <summary>
    /// Opens a page as an overlay.
    /// </summary>
    /// <param name="page">The page to open.</param>
    public void OpenPage(PageViewModel page)
    {
        // Push the current page to the stack.
        if (CurrentPage is not null)
        {
            _navStack.Push(CurrentPage);
        }

        CurrentPage = page;
    }

    /// <summary>
    /// Navigates back a page.
    /// </summary>
    public void GoBack()
    {
        var prevPage = _navStack.Count is not 0 ? _navStack.Pop() : null;
        CurrentPage = prevPage;
    }

    /// <summary>
    /// Closes the current resource.
    /// </summary>
    public void Close()
    {
        CurrentPage = null;
        _navStack.Clear();
    }
}
