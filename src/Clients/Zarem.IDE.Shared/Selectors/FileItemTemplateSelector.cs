// Avishai Dernis 2024

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Zarem.Bindables.Files;

namespace Zarem.IDE.Selectors;

/// <summary>
/// A <see cref="DataTemplateSelector"/> for 
/// </summary>
public partial class FileItemTemplateSelector : DataTemplateSelector
{
    /// <summary>
    /// Gets the <see cref="DataTemplate"/> for a <see cref="BindableFile"/>.
    /// </summary>
    public DataTemplate? FileTemplate { get; set; }

    /// <summary>
    /// Gets the <see cref="DataTemplate"/> for a <see cref="BindableFolder"/>.
    /// </summary>
    public DataTemplate? FolderTemplate { get; set; }

    /// <summary>
    /// Gets the <see cref="DataTemplate"/> for a <see cref="BindableProjectFile"/>.
    /// </summary>
    public DataTemplate? ProjectTemplate { get; set; }
    
    /// <inheritdoc/>
    protected override DataTemplate? SelectTemplateCore(object item)
    {
        return item switch
        {
            BindableFile => FileTemplate,
            BindableFolder => FolderTemplate,
            BindableProjectFile => ProjectTemplate,
            _ => null,
        };
    }

    /// <inheritdoc/>
    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container) => base.SelectTemplateCore(item);
}
