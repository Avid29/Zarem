// Avishai Dernis 2026

using System.Threading.Tasks;

namespace Zarem.IDE.ViewModels.Pages.Interfaces;

/// <summary>
/// An interface for a file editor view implementation.
/// </summary>
public interface IFileEditorHandler
{
    /// <summary>
    /// Saves the contents of the editing file.
    /// </summary>
    Task<bool> SaveAsync();

    /// <summary>
    /// Gets whether or not the editor state is dirty.
    /// </summary>
    bool IsDirty { get; }
}
