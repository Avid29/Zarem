// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System.Threading.Tasks;
using Zarem.Assembler;
using Zarem.Assembler.Config;
using Zarem.Assembler.Handlers;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Models;
using Zarem.Components.Interfaces;
using Zarem.Descriptors;
using Zarem.Models.Files;

namespace Zarem.Components;

/// <summary>
/// A component of a <see cref="Project"/> class for assembling assembly code.
/// </summary>
public class AssembleComponent<THandler, TConfig> : IAssembleComponent
    where THandler : IAssemblerHandler<TConfig>
    where TConfig : AssemblerConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssembleComponent{TAssembler, TConfig}"/> class.
    /// </summary>
    public AssembleComponent(THandler handler, TConfig config, IAssemblerDescriptor descriptor)
    {
        Handler = handler;
        Config = config;
    }

    /// <inheritdoc/>
    public TConfig Config { get; }

    /// <inheritdoc/>
    public IAssemblerHandler Handler { get; }

    /// <inheritdoc/>
    AssemblerConfig IAssembleComponent.Config => Config;

    /// <inheritdoc/>
    public async Task<AssemblerResult?> AssembleFileAsync(SourceFile file, bool rebuild = true, Logger? logger = null)
    {
        // Skip if not dirty and not rebuilding
        if (!file.IsDirty && !rebuild)
            return null;

        Guard.IsNotNull(Config);

        var result = await Zarembler.AssembleAsync(file.FullPath, Handler, Config, logger);
        return result;
    }
}
