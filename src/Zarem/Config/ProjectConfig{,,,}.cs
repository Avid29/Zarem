// Avishai Dernis 2025

using System.IO;
using System.Xml.Serialization;

namespace Zarem.Config;

/// <summary>
/// A model for project configurations.
/// </summary>
[XmlRoot("Project")]
public partial class ProjectConfig : IProjectConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IProjectConfig"/> class.
    /// </summary>
    public ProjectConfig()
    {

    }

    /// <inheritdoc/>
    public string? Name { get; init; }

    /// <inheritdoc/>
    [XmlIgnore]
    public string? ConfigPath { get; set; }

    /// <inheritdoc/>
    [XmlIgnore]
    public string? RootFolderPath => Path.GetDirectoryName(ConfigPath);

    /// <inheritdoc/>
    public IArchitectureConfig? ArchitectureConfig { get; init; }

    /// <inheritdoc/>
    public FormatConfig? FormatConfig { get; init; }
}
