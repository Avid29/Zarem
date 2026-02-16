// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Zarem.Assembler;
using Zarem.Assembler.Config;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Models;
using Zarem.Components.Interfaces;
using Zarem.Models.Files;
using Zarem.Registry.Descriptors;

namespace Zarem.Components;

/// <summary>
/// A component of a <see cref="Project"/> class for assembling assembly code.
/// </summary>
/// <typeparam name="TAsmHandler"></typeparam>
/// <typeparam name="TConfig"></typeparam>
public class AssembleComponent<TAsmHandler, TConfig> : IAssembleComponent
    where TAsmHandler : IArchHandler
    where TConfig : AssemblerConfig
{
    private TAsmHandler _asmHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssembleComponent{TAssembler, TConfig}"/> class.
    /// </summary>
    public AssembleComponent(TAsmHandler asmHandler, TConfig config, IAssemblerDescriptor descriptor)
    {
        _asmHandler = asmHandler;
        Config = config;
    }

    /// <inheritdoc/>
    public TConfig Config { get; }

    AssemblerConfig IAssembleComponent.Config => Config;

    /// <inheritdoc/>
    public async Task<AssemblerResult?> AssembleFileAsync(SourceFile file, bool rebuild = true, Logger? logger = null)
    {
        // Skip if not dirty and not rebuilding
        if (!file.IsDirty && !rebuild)
            return null;

        Guard.IsNotNull(Config);

        using var stream = File.OpenRead(file.FullPath);
        var result = await Zarembler.AssembleAsync(stream, file.Name, _asmHandler, Config, logger);
        return result;
    }
}
