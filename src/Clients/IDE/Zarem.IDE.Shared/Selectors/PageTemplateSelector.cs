// Avishai Dernis 2025

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Zarem.IDE.ViewModels.Pages;
using Zarem.IDE.ViewModels.Pages.CheatSheet;
using Zarem.IDE.ViewModels.Pages.Settings;

namespace Zarem.IDE.Selectors;

public partial class PageTemplateSelector : DataTemplateSelector
{
    /// <summary>
    /// Gets the <see cref="DataTemplate"/> for a <see cref="CheatSheetViewModel"/>."/>
    /// </summary>
    public DataTemplate? AboutPageTemplate { get; set; }

    /// <summary>
    /// Gets the <see cref="DataTemplate"/> for a <see cref="CheatSheetViewModel"/>."/>
    /// </summary>
    public DataTemplate? CheatSheetPageTemplate { get; set; }

    /// <summary>
    /// Gets the <see cref="DataTemplate"/> for a <see cref="CreateProjectViewModel"/>."/>
    /// </summary>
    public DataTemplate? CreateProjectPageTemplate { get; set; }

    /// <summary>
    /// Gets the <see cref="DataTemplate"/> for a <see cref="FilePageViewModel"/>.
    /// </summary>
    public DataTemplate? FilePageTemplate { get; set; }

    /// <summary>
    /// Gets the <see cref="DataTemplate"/> for a <see cref="GraphicalOutputPageViewModel"/>.
    /// </summary>
    public DataTemplate? GraphicalOuputPageTemplate { get; set; }

    /// <summary>
    /// Gets the <see cref="DataTemplate"/> for a <see cref="SettingsPageViewModel"/>."/>
    /// </summary>
    public DataTemplate? SettingsPageTemplate { get; set; }

    /// <summary>
    /// Gets the <see cref="DataTemplate"/> for a <see cref="WelcomePageViewModel"/>."/>
    /// </summary>
    public DataTemplate? WelcomePageTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
    {
        return item switch
        {
            AboutPageViewModel => AboutPageTemplate,
            CheatSheetViewModel => CheatSheetPageTemplate,
            CreateProjectViewModel => CreateProjectPageTemplate,
            FilePageViewModel => FilePageTemplate,
            GraphicalOutputPageViewModel => GraphicalOuputPageTemplate,
            SettingsPageViewModel => SettingsPageTemplate,
            WelcomePageViewModel => WelcomePageTemplate,
            _ => null,
        };
    }

    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container) => this.SelectTemplateCore(item);
}
