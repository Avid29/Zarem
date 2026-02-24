// Avishai Dernis 2025

using Zarem.Models;

namespace Zarem.Messages.Build;

/// <summary>
/// A message sent when the build finished, containing status info.
/// </summary>
public class BuildFinishedMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BuildFinishedMessage"/> class.
    /// </summary>
    public BuildFinishedMessage(BuildResult? result)
    {
        Result = result;
    }

    /// <summary>
    /// Gets the build result.
    /// </summary>
    public BuildResult? Result { get; }
}
