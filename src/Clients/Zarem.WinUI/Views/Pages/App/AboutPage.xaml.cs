// Avishai Dernis 2025

using Microsoft.UI.Xaml.Controls;
using Zarem.ViewModels.Pages.App;

namespace Zarem.WinUI.Views.Pages.App;

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
}
