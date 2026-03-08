// Avishai Dernis 2025

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.IO;
using Zarem.IDE.ViewModels.Pages;

namespace Zarem.IDE.Selectors;

public partial class EditorTemplateSelector : DataTemplateSelector
{
    /// <summary>
    /// Gets the <see cref="DataTemplate"/> for an assembly editor.
    /// </summary>
    public DataTemplate? TextEditorTemplate { get; set; }

    /// <summary>
    /// Gets the <see cref="DataTemplate"/> for the module editor.
    /// </summary>
    public DataTemplate? ModuleEditorTemplate { get; set; }
    
    /// <inheritdoc/>
    protected override DataTemplate? SelectTemplateCore(object item)
    {
        if (item is not FilePageViewModel filePage)
            return null;

        // Use text editor if forced
        if (filePage.ForceTextEditor)
            return TextEditorTemplate;

        // Attempt to retrieve a path
        var path = filePage.File?.Path;
        if (path is null)
            return null;

        // Get file type
        var type = Path.GetExtension(path);

        // TODO: Make configurable
        return type switch
        {
            ".obj" or "" => ModuleEditorTemplate,
            _ => TextEditorTemplate,
        };
    }

    /// <inheritdoc/>
    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container) => this.SelectTemplateCore(item);
}
