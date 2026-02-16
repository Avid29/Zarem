// Adam Dernis 2024

using System.IO;
using System.Threading.Tasks;
using Zarem.Models.Interface;

namespace Zarem.Assembler.Models;

/// <summary>
/// An interface for a module implementation.
/// </summary>
public interface IBuildModule : IModule
{
    /// <summary>
    /// Gets the name of the <see cref="IBuildModule"/>.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Save the module to a stream (likely as a file).
    /// </summary>
    public Task SaveAsync(Stream stream);
}
