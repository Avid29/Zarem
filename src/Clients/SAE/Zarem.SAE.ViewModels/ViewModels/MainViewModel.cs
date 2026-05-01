// Avishai Dernis 2026

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Zarem.Emulator.Config.Enums;
using Zarem.N64;
using Zarem.N64.Config;

namespace Zarem.SAE.ViewModels;

/// <summary>
/// The main view model for the application.
/// </summary>
public partial class MainViewModel : ObservableRecipient
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    public MainViewModel()
    {
        var config = new N64EmulatorConfig(ExecutionMode.Interpret);
        N64 = new Nintendo64(config);
    }

    /// <summary>
    /// Gets the Nintendo 64 emulator instance.
    /// </summary>
    public Nintendo64 N64 { get; }
}
