// Avishai Dernis 2025

using System.IO;

namespace Zarem.Config;

/// <summary>
/// A model for project configurations.
/// </summary>
public interface IProjectConfig : IConfig
{   
    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    string? Name { get; init; }

    /// <summary>
    /// Gets or sets the path for the config file.
    /// </summary>
    string? ConfigPath { get; set; }

    /// <summary>
    /// Gets the path root folder path.
    /// </summary>
    string? RootFolderPath => Path.GetDirectoryName(ConfigPath);

    /// <summary>
    /// Gets the assembler configuration for the project.
    /// </summary>
    IArchitectureConfig? ArchitectureConfig { get; }

    /// <summary>
    /// Gets the format configuration for the project.
    /// </summary>
    FormatConfig? FormatConfig { get; init;  }
}
