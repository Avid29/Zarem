// Avishai Dernis 2025

using Zarem.Services;
using Zarem.ViewModels.Pages.Abstract;
using System.Linq;

namespace Zarem.ViewModels;

public partial class MainViewModel
{
    /// <summary>
    /// Navigates to a page by type.
    /// </summary>
    /// <param name="open">Open page if not already open.</param>
    public void GoToPageByType<T>(bool open = true)
        where T : PageViewModel
    {
        // Check if the page is already open, and open it if not
        var page = FocusedPanel?.OpenPages.FirstOrDefault(p => p is T);
        if (page is null && open)
        {
            page = Service.Get<T>();
            FocusedPanel?.OpenPages.Add(page);
        }

        // Navigate to the page
        if (FocusedPanel is not null)
        {
            FocusedPanel.CurrentPage = page;
        }
    }
}
