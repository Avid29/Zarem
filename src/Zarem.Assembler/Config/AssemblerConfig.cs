// Avishai Dernis 2024

using Zarem.Config;

namespace Zarem.Assembler.Config;

/// <summary>
/// A base class for an assembler configuration.
/// </summary>
public abstract class AssemblerConfig : IConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblerConfig"/> class.
    /// </summary>
    public AssemblerConfig()
    {
    }

    /// <summary>
    /// Gets the threshold alignment size where the assembler will give a message.
    /// </summary>
    public int? AlignMessageThreshold { get; set; }

    /// <summary>
    /// Gets the threshold alignment size where the assembler will give a warning.
    /// </summary>
    public int? AlignWarningThreshold { get; set; }

    /// <summary>
    /// Gets the threshold alignment size where the assembler will give a warning.
    /// </summary>
    public int? SpaceMessageThreshold { get; set; }
}
