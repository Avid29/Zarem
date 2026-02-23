// Adam Dernis 2024

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Zarem.Services;
using Zarem.Services.Popup;
using Zarem.Services.Popup.Enums;
using Zarem.Services.Popup.Models;
using Zarem.ViewModels.Pages.Abstract;

namespace Zarem.ViewModels;

/// <summary>
/// A view model for tracking the open files.
/// </summary>
public class PanelViewModel : ObservableObject
{
    private readonly ILocalizationService _localizationService;
    private readonly IPopupService _popupService;
     
    /// <summary>
    /// Initializes a new instance of the <see cref="PanelViewModel"/> class.
    /// </summary>
    public PanelViewModel(MainViewModel mainViewModel, ILocalizationService localizationService, IPopupService popupService)
    {
        _localizationService = localizationService;
        _popupService = popupService;

        OpenPages = [];

        mainViewModel.Panels.Add(this);
    }

    /// <summary>
    /// Gets or sets the currently selected file.
    /// </summary>
    public PageViewModel? CurrentPage
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                OnPropertyChanged(nameof(IsPageOpen));
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether or not a page is open.
    /// </summary>
    public bool IsPageOpen => CurrentPage is not null;

    /// <summary>
    /// Gets an <see cref="ObservableCollection{T}"/> of open files.
    /// </summary>
    public ObservableCollection<PageViewModel> OpenPages { get; }

    /// <summary>
    /// Attempts to save the current file.
    /// </summary>
    /// <remarks>
    /// Does nothing if the current page is not a file.
    /// </remarks>
    public async Task SaveFileAsync()
    {
        if (CurrentPage is null || !CurrentPage.CanSave)
            return;

        await CurrentPage.SaveAsync();
    }

    /// <summary>
    /// Attempts to save the currently open files.
    /// </summary>
    public async Task SaveAllFilesAsync()
    {
        foreach(var page in OpenPages)
        {
            if (page.CanSave)
            {
                await page.SaveAsync();
            }
        }
    }

    /// <summary>
    /// Opens a page.
    /// </summary>
    public void OpenPage(PageViewModel page)
    {
        // Open the page if needed
        if (!OpenPages.Contains(page))
            OpenPages.Add(page);

        CurrentPage = page;
    }

    /// <summary>
    /// Closes a page.
    /// </summary>
    /// <remarks>
    /// Does not save the file.
    /// </remarks>
    public async Task ClosePageAsync(PageViewModel? page, bool confirmIfDirty = true)
    {
        page ??= CurrentPage;
        if (page is null)
            return;

        bool confirm = confirmIfDirty && page.IsDirty;
        var confirmation = PopupResult.Secondary;

        if (confirm)
        {
            var title = _localizationService["/Popups/UnsavedChangesTitle", page.Title];
            var desc = _localizationService["/Popups/UnsavedChangesDescription"];
            var popup = new PopupDetails(title, desc)
            {
                PrimaryButtonText = _localizationService["/Popups/Save"],
                SecondaryButtonText = _localizationService["/Popups/DontSave"],
                CloseButtonText = _localizationService["/Popups/Cancel"],
            };

            confirmation = await _popupService.ShowPopupAsync(popup);
        }


        // Cancel operation if popup ignored
        if (confirmation is PopupResult.Closed)
            return;

        // We can now close the page
        ClosePage(page);

        // Save changes if save was selected
        if (confirmation is PopupResult.Primary)
            await page.SaveAsync();
    }

    /// <summary>
    /// Closes a page.
    /// </summary>
    /// <remarks>
    /// Does not save the file.
    /// </remarks>
    public void ClosePage(PageViewModel? page)
    {
        page ??= CurrentPage;
        if (page is null)
            return;

        OpenPages.Remove(page);
    }
}
