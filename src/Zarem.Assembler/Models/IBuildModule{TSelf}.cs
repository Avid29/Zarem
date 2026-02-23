// Avishai Dernis 2024

using System.IO;
using Zarem.Config;
using Zarem.Models;

namespace Zarem.Assembler.Models;

/// <summary>
/// An interface for a module implementation with knowledge of the underlying type.
/// </summary>
public interface IBuildModule<TSelf, TConfig> : IBuildModule
    where TSelf : IBuildModule<TSelf, TConfig>
    where TConfig : FormatConfig
{
    /// <summary>
    /// Abstracts the module into a <see cref="Module"/> for modification or linking.
    /// </summary>
    /// <param name="config">The configuration settings.</param>
    /// <returns>The module as a <see cref="Module"/>.</returns>
    public Module? Abstract(TConfig config);

    /// <summary>
    /// Creates a module from a <see cref="Module"/>.
    /// </summary>
    /// <param name="module">The <see cref="Module"/> to build from.</param>
    /// <param name="config">The configuration settings.</param>
    /// <returns>The constructed module.</returns>
    public static abstract TSelf? Create(Module module, TConfig config);

    /// <summary>
    /// Opens a module from a stream.
    /// </summary>
    /// <returns>The module contained in the stream.</returns>
    public static abstract TSelf? Open(string name, Stream stream);
}
