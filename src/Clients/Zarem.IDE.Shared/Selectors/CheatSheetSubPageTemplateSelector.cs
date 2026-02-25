// Avishai Dernis 2025

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Zarem.IDE.ViewModels.Pages.CheatSheet;
using Zarem.IDE.ViewModels.Pages.Settings;

namespace Zarem.IDE.Selectors;

public partial class CheatSheetSubPageTemplateSelector : DataTemplateSelector
{
    /// <summary>
    /// Gets the <see cref="DataTemplate"/> for a <see cref="AppSettingsViewModel"/>."/>
    /// </summary>
    public DataTemplate? UsagePatternPageTemplate { get; set; }

    /// <summary>
    /// Gets the <see cref="DataTemplate"/> for a <see cref="EditorSettingsViewModel"/>."/>
    /// </summary>
    public DataTemplate? EncodingPatternsPageTemplate { get; set; }

    /// <summary>
    /// Gets the <see cref="DataTemplate"/> for a <see cref="AssemblerSettingsViewModel"/>.
    /// </summary>
    public DataTemplate? EncodingTablesPageTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
    {
        return item switch
        {
            UsagePatternsViewModel => UsagePatternPageTemplate,
            EncodingPatternsViewModel => EncodingPatternsPageTemplate,
            EncodingTablesViewModel => EncodingTablesPageTemplate,
            _ => null,
        };
    }

    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container) => this.SelectTemplateCore(item);
}
