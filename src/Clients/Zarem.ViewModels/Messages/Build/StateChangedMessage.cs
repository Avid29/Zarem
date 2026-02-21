// Avishai Dernis 2025

using Zarem.Models.Enums;

namespace Zarem.Messages.Build;

/// <summary>
/// A message sent when the build status changes.
/// </summary>
public class StateChangedMessage
{
    /// <summary>
    /// Initialzes a new instance of the <see cref="StateChangedMessage"/> class.
    /// </summary>
    public StateChangedMessage(IdeState status, string? message = null)
    {
        State = status;
        Message = message;
    }

    /// <summary>
    /// Gets the new build status.
    /// </summary>
    public IdeState State { get; }

    /// <summary>
    /// Gets the build status message.
    /// </summary>
    public string? Message { get; }
}
