// Avishai Dernis 2025

using Microsoft.UI.Xaml.Controls;
using Zarem.ViewModels.Pages.App;

namespace Zarem.IDE.Views.Pages.App;

/// <summary>
/// A viewer for files.
/// </summary>
public sealed partial class WelcomePage : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WelcomePage"/> class.
    /// </summary>
    public WelcomePage()
    {
        this.InitializeComponent();
    }

    public WelcomePageViewModel? ViewModel { get; set; }
}
