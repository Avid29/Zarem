// Avishai Dernis 2025

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.System;
using Zarem.Models;
using Zarem.ViewModels.Pages.App;

namespace Zarem.IDE.Views.Pages.App;

/// <summary>
/// A viewer for files.
/// </summary>
public sealed partial class AboutPage : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AboutPage"/> class.
    /// </summary>
    public AboutPage()
    {
        this.InitializeComponent();
    }

    public AboutPageViewModel? ViewModel { get; set; }

    private async void SettingsCard_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element)
            return;

        if (element.DataContext is not ThirdPartyNotice notice)
            return;

        await Launcher.LaunchUriAsync(new Uri(notice.Url));
    }
}
