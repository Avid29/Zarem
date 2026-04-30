// Avishai Dernis 2026

using System;
using System.Collections.ObjectModel;
using Zarem.IDE.Bindables.Files.Interfaces;

namespace Zarem.IDE.Bindables.Files;

/// <summary>
/// A <see cref="ObservableCollection{T}"/> for <see cref="IBindableFileItem"/>s.
/// </summary>
public class BindableFileItemCollection : ObservableCollection<IBindableFileItem>
{
    /// <summary>
    /// Adds the item to the collection.
    /// </summary>
    public new void Add(IBindableFileItem item) => Insert(0, item);

    /// <inheritdoc/>
    protected override void InsertItem(int index, IBindableFileItem item)
    {
        int sortedIndex = GetSortedIndex(item);
        base.InsertItem(sortedIndex, item);
    }

    private int GetSortedIndex(IBindableFileItem newItem)
    {
        int low = 0;
        int high = Count;

        while (low < high)
        {
            int mid = (low + high) / 2;
            if (CompareItems(Items[mid], newItem) < 0)
                low = mid + 1;
            else
                high = mid;
        }
        return low;
    }

    private int CompareItems(IBindableFileItem existing, IBindableFileItem newItem)
    {
        // Folders go above files
        if (existing.IsFolder && !newItem.IsFolder)
            return -1;

        // Files go below folders
        if (!existing.IsFolder && newItem.IsFolder)
            return 1;  

        // After being grouped, folders and files are sorted alphabetically
        return string.Compare(existing.Name, newItem.Name, StringComparison.CurrentCultureIgnoreCase);
    }
}
