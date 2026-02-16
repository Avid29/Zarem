// Avishai Dernis 2026

using Zarem.Assembler.Config;

namespace Zarem.Assembler.Handlers;

/// <summary>
/// An interface for an handling architecture specific assembler functions.
/// </summary>
public interface IAssemblerHandler<TConfig> : IAssemblerHandler
    where TConfig : AssemblerConfig
{
    /// <summary>
    /// Gets the assembler config for the handler.
    /// </summary>
    TConfig Config { get; }
}
