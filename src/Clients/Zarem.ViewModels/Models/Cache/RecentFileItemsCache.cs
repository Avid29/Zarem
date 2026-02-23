// Avishai Dernis 2025

using System.Collections.Generic;

namespace Zarem.Models.Cache;

/// <summary>
/// A model containing caches for recently open files, folders, or projects.
/// </summary>
public class RecentFileItemsCache
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecentFileItemsCache"/> class.
    /// </summary>
    public RecentFileItemsCache()
    {
        Paths = [];
    }

    /// <summary>
    /// Gets the paths of the file items.
    /// </summary>
    public LinkedList<string> Paths { get; set;  }

    /// <summary>
    /// Appends an item to the recent files cache.
    /// </summary>
    /// <param name="path">The item to append.</param>
    /// <param name="maxSize">The max size of the cache.</param>
    public void Append(string? path, int maxSize)
    {
        if (path is null)
            return;

        var pathAsNode = Paths.Find(path);
        if (pathAsNode is not null)
        {
            Paths.Remove(pathAsNode);
            Paths.AddFirst(pathAsNode);
        } else
        {
            Paths.AddFirst(path);
        }

        while (Paths.Count > maxSize)
            Paths.RemoveFirst();
    }
}
