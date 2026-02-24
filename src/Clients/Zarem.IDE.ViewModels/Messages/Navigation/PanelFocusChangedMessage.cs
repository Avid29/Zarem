// Avishai Dernis 2025

using Zarem.ViewModels;

namespace Zarem.Messages.Navigation;

/// <summary>
/// A message indicating that the focused panel has changed.
/// </summary>
public class PanelFocusChangedMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PanelFocusChangedMessage"/> class.
    /// </summary>
    public PanelFocusChangedMessage(PanelViewModel panel)
    {
        FocusedPanel = panel;
    }

    /// <summary>
    /// Gets the newly focused <see cref="PanelViewModel"/>.
    /// </summary>
    public PanelViewModel FocusedPanel { get; }
}
