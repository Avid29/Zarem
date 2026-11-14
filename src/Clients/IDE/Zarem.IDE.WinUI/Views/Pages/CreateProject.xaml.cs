// Avishai Dernis 2024

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Zarem.IDE.ViewModels.Pages;

namespace Zarem.IDE.Views.Pages;

/// <summary>
/// A create project view.
/// </summary>
public sealed partial class CreateProject : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateProject"/> class.
    /// </summary>
    public CreateProject()
    {
        this.InitializeComponent();
    }

    public CreateProjectViewModel? ViewModel { get; set; }

    public static Visibility VisibleNotNull(object? obj) => obj is not null ? Visibility.Visible : Visibility.Collapsed;

    public static bool IsNotNull(object? obj) => obj is not null;
}
