// Avishai Dernis 2025

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Zarem.SAE.ViewModels.Pages;
using Zarem.SAE.ViewModels.Pages.Settings;

namespace Zarem.SAE.Selectors;

public partial class PageTemplateSelector : DataTemplateSelector
{
    /// <summary>
    /// Gets the <see cref="DataTemplate"/> for a <see cref="CheatSheetViewModel"/>."/>
    /// </summary>
    public DataTemplate? AboutPageTemplate { get; set; }

    /// <summary>
    /// Gets the <see cref="DataTemplate"/> for a <see cref="SettingsPageViewModel"/>."/>
    /// </summary>
    public DataTemplate? SettingsPageTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
    {
        return item switch
        {
            SaeAboutPageViewModel => AboutPageTemplate,
            SaeSettingsPageViewModel => SettingsPageTemplate,
            _ => null,
        };
    }

    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container) => this.SelectTemplateCore(item);
}
