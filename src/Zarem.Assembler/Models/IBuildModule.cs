// Avishai Dernis 2024

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
    /// Save the module to a stream (likely as a file).
    /// </summary>
    Task SaveAsync(Stream stream);
}
