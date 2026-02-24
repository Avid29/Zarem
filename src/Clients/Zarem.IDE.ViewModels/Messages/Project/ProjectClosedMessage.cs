// Avishai Dernis 2026

namespace Zarem.IDE.Messages.Project;

/// <summary>
/// A message sent a project is closed.
/// </summary>
public class ProjectClosedMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectClosedMessage"/> class.
    /// </summary>
    public ProjectClosedMessage(IProject project)
    {
        Project = project;
    }

    /// <summary>
    /// Gets the project that was closed.
    /// </summary>
    public IProject Project { get; }
}
