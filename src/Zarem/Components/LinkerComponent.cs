// Avishai Dernis 2026

using Zarem.Assembler.Logging;
using Zarem.Components.Interfaces;
using Zarem.Descriptors;
using Zarem.Linker;
using Zarem.Linker.Config;
using Zarem.Linker.Handlers;
using Zarem.Models;

namespace Zarem.Components;

/// <summary>
/// A component of a <see cref="Project"/> that emulates machines.
/// </summary>
public class LinkerComponent<TLinkerComponent, TConfig> : ILinkerComponent
    where TLinkerComponent : ILinkerHandler<TConfig>
    where TConfig : LinkerConfig
{
    private readonly TLinkerComponent _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssembleComponent{TAssembler, TConfig}"/> class.
    /// </summary>
    public LinkerComponent(TLinkerComponent handler, TConfig config, ILinkerDescriptor descriptor)
    {
        _handler = handler;
        Config = config;
    }

    /// <inheritdoc/>
    public TConfig Config { get; }

    LinkerConfig ILinkerComponent.Config => Config;

    /// <inheritdoc/>
    public Module Link(Logger? logger, params Module[] modules) => ZaLinker.Link(Config, _handler, logger, modules);
}
