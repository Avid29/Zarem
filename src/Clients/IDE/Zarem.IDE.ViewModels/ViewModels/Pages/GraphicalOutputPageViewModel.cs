// Avishai Dernis 2026

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Linq;
using Zarem.DebugSessions;
using Zarem.Emulator.Devices.Interfaces;
using Zarem.IDE.ViewModels.Pages.Abstract;

namespace Zarem.IDE.ViewModels.Pages;

/// <summary>
/// A view model for the graphical output viewer.
/// </summary>
public partial class GraphicalOutputPageViewModel : DebugPageViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GraphicalOutputPageViewModel"/> class.
    /// </summary>
    public GraphicalOutputPageViewModel(IMessenger messenger) : base(messenger)
    {
        GraphicsDevice = null;
    }

    /// <inheritdoc/>
    public override string Title => "Graphics Viewer"; // TODO: Localization

    /// <summary>
    /// Gets or sets the graphics device being viewed.
    /// </summary>
    [ObservableProperty]
    public partial IGraphicsDevice? GraphicsDevice { get; set; }

    /// <inheritdoc/>
    protected override void RegisterSession(DebugSession session)
    {
        GraphicsDevice = session.Emulator.Computer.Devices
            .OfType<IGraphicsDevice>()
            .FirstOrDefault();
    }

    /// <inheritdoc/>
    protected override void UnregisterSession()
    {
        GraphicsDevice = null;
    }
}
