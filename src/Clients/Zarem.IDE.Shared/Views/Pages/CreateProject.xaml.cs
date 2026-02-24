// Avishai Dernis 2024

using Microsoft.UI.Xaml.Controls;
using Zarem.ViewModels.Pages;

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
}
