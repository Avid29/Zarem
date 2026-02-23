// Avishai Dernis 2025

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Zarem.Services.Popup;
using Zarem.Services.Popup.Enums;
using Zarem.Services.Popup.Models;
using System;
using System.Threading.Tasks;
using Zarem.Services;

namespace Zarem.WinUI.Services.Popup;

public class PopupService : IPopupService
{
    private readonly ILocalizationService _localizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PopupService"/> class.
    /// </summary>
    /// <param name="localizationService"></param>
    public PopupService(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    /// <inheritdoc/>
    public async Task<PopupResult> ShowPopupAsync(PopupDetails popup)
    {
        // TODO: Multi-Windowing
        var xamlRoot = App.Current.Window?.Content.XamlRoot;
        if (xamlRoot is null)
        {
            return PopupResult.Closed;
        }

        var dialog = new ContentDialog
        {
            Title = popup.Title,
            Content = popup.Description,
            PrimaryButtonText = popup.PrimaryButtonText,
            PrimaryButtonStyle = (Style)App.Current.Resources["AccentButtonStyle"],
            IsPrimaryButtonEnabled = popup.PrimaryButtonText is not null,
            SecondaryButtonText = popup.SecondaryButtonText,
            IsSecondaryButtonEnabled = popup.SecondaryButtonText is not null,
            CloseButtonText = popup.CloseButtonText,
            XamlRoot = xamlRoot,
            FlowDirection = FlowDirection,
        };

        var result = await dialog.ShowAsync();
        return result switch
        {
            ContentDialogResult.None => PopupResult.Closed,
            ContentDialogResult.Primary => PopupResult.Primary,
            ContentDialogResult.Secondary => PopupResult.Secondary,
            _ => PopupResult.Closed,
        };
    }

    /// <inheritdoc/>
    public async Task<string?> ShowPopupAsync(TextInputPopupDetails popup)
    {
        // TODO: Multi-Windowing
        var xamlRoot = App.Current.Window?.Content.XamlRoot;
        if (xamlRoot is null)
            return null;

        var dialog = new TextInputPopup()
        {
            Title = popup.Title,
            Description = popup.Description,
            ValidatonRegex = popup.ValidationRegex,
            PrimaryButtonText = popup.PrimaryButtonText,
            PrimaryButtonStyle = (Style)App.Current.Resources["AccentButtonStyle"],
            IsPrimaryButtonEnabled = popup.PrimaryButtonText is not null,
            SecondaryButtonText = popup.SecondaryButtonText,
            IsSecondaryButtonEnabled = popup.SecondaryButtonText is not null,
            CloseButtonText = popup.CloseButtonText,
            XamlRoot = xamlRoot,
            FlowDirection = FlowDirection
        };

        var result = await dialog.ShowAsync();
        return result switch
        {
            ContentDialogResult.None => null,
            ContentDialogResult.Primary => dialog.Text,
            _ => null,
        };
    }

    private FlowDirection FlowDirection => _localizationService.IsRightToLeftLanguage ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
}
