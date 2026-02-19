// Avishai Dernis 2026

using Zarem.Assembler.Handlers;
using Zarem.Linker.Config;

namespace Zarem.Linker.Handlers;

/// <summary>
/// An interface for an architecture-specific linker handler.
/// </summary>
public interface ILinkerHandler<TConfig> : ILinkerHandler
    where TConfig : LinkerConfig
{
    /// <summary>
    /// Gets the assembler config for the handler.
    /// </summary>
    TConfig Config { get; }
}
