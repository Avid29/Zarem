// Avishai Dernis 2025

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Globalization;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Messages.Build;
using Zarem.Messages.Navigation;
using Zarem.Services;
using Zarem.Services.Settings;
using Zarem.ViewModels.Pages;

namespace Zarem.WinUI.Views.Pages;

/// <summary>
/// A viewer for files.
/// </summary>
public sealed partial class ErrorList : UserControl
{
    private static readonly DependencyProperty MessageFlowDirectionProperty =
        DependencyProperty.Register(nameof(MessageFlowDirection), typeof(FlowDirection), typeof(ErrorList), new(FlowDirection.LeftToRight));

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorList"/> class.
    /// </summary>
    public ErrorList()
    {
        this.InitializeComponent();

        ViewModel = Service.Get<ErrorListViewModel>();
        DataContext = this;

        UpdateAlignment();

        // Update alignment when the build finishes in case the assembler language changed.
        Service.Get<IMessenger>().Register<ErrorList, BuildFinishedMessage>(this, (r, m) => r.UpdateAlignment());
    }

    private ErrorListViewModel ViewModel { get; }

    private static string DisplayLine(SourceLocation? location)
    {
        // The location is null. Display nothing
        if (location is null)
        {
            return string.Empty;
        }

        // Get the line
        return $"{location.Value.Line}";
    }

    private void ListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not AssemblerEntry log)
            return;

        var messenger = Service.Get<IMessenger>();
        messenger.Send(new NavigateToTokenRequestMessage(log.Tokens[0]));
    }

    public FlowDirection MessageFlowDirection
    {
        get => (FlowDirection)GetValue(MessageFlowDirectionProperty);
        set => SetValue(MessageFlowDirectionProperty, value);
    }

    private void UpdateAlignment()
    {
        var culture = CultureInfo.CurrentUICulture;
        var lang = Service.Get<ISettingsService>().Local.GetValue<string>(SettingsKeys.AssemblerLanguageOverride);
        if (lang is not null)
        {
            culture = CultureInfo.GetCultureInfo(lang);
        }

        var isRtl = culture.TextInfo.IsRightToLeft;
        MessageFlowDirection = isRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
    }
}
