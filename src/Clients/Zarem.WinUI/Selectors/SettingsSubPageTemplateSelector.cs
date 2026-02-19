// Avishai Dernis 2025

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Zarem.ViewModels.Pages.App.Settings;

namespace Zarem.WinUI.Selectors;

public partial class SettingsSubPageTemplateSelector : DataTemplateSelector
{
    /// <summary>
    /// Gets the <see cref="DataTemplate"/> for a <see cref="AppSettingsViewModel"/>."/>
    /// </summary>
    public DataTemplate? AppPageTemplate { get; set; }

    /// <summary>
    /// Gets the <see cref="DataTemplate"/> for a <see cref="EditorSettingsViewModel"/>."/>
    /// </summary>
    public DataTemplate? EditorPageTemplate { get; set; }

    /// <summary>
    /// Gets the <see cref="DataTemplate"/> for a <see cref="AssemblerSettingsViewModel"/>.
    /// </summary>
    public DataTemplate? AssemblerPageTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
    {
        return item switch
        {
            AppSettingsViewModel => AppPageTemplate,
            EditorSettingsViewModel => EditorPageTemplate,
            AssemblerSettingsViewModel => AssemblerPageTemplate,
            _ => null,
        };
    }

    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container) => this.SelectTemplateCore(item);
}
