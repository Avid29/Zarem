// Avishai Dernis 2026

namespace Zarem.Messages;

/// <summary>
/// A message sent a project is opened (or closed).
/// </summary>
public class ProjectOpenedMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectOpenedMessage"/> class.
    /// </summary>
    public ProjectOpenedMessage(IProject project)
    {
        Project = project;
    }

    /// <summary>
    /// Gets the project that was opened.
    /// </summary>
    public IProject Project { get; }
}
