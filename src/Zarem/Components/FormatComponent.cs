// Avishai Dernis 2026

using System.IO;
using System.Threading.Tasks;
using Zarem.Assembler.Models;
using Zarem.Components.Interfaces;
using Zarem.Config;
using Zarem.Descriptors;
using Zarem.Emulator.Models.Modules;
using Zarem.Models;
using Zarem.Models.Files;

namespace Zarem.Components;

/// <summary>
/// A component of a <see cref="Project"/> that exports formatted binaries.
/// </summary>
/// <typeparam name="TModule">The target module format's type.</typeparam>
/// <typeparam name="TConfig">The type for the format's config.</typeparam>
public class FormatComponent<TModule, TConfig> : IFormatComponent
    where TModule : IBuildModule<TModule, TConfig>, IExecutableModule
    where TConfig : FormatConfig
{
    /// <summary>
    /// Initialize a new instance of the <see cref="FormatComponent{TModule, TConfig}"/> class.
    /// </summary>
    public FormatComponent(TConfig config, IModuleFormatDescriptor descriptor)
    {
        Config = config;
    }

    /// <inheritdoc/>
    public TConfig Config { get; }

    FormatConfig IFormatComponent.Config => Config;

    /// <inheritdoc/>
    public async Task<bool> TryExportAsync(Module module, ObjectFile @object)
    {
        var export = TModule.Create(module, Config);
        if (export is null)
            return false;

        using var outstream = File.Open(@object.FullPath, FileMode.OpenOrCreate);
        await export.SaveAsync(outstream);
        return true;
    }

    /// <inheritdoc/>
    public async Task<IExecutableModule?> ImportAsync(ObjectFile @object)
    {
        using var instream = File.OpenRead(@object.FullPath);
        return TModule.Open(@object.Name, instream);
    }
}
