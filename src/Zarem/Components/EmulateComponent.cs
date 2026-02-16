// Avishai Dernis 2026

using Zarem.Components.Interfaces;
using Zarem.Descriptors;
using Zarem.Emulator;
using Zarem.Emulator.Config;

namespace Zarem.Components;

/// <summary>
/// A component of a <see cref="Project"/> that emulates machines.
/// </summary>
/// <typeparam name="TEmulator">The emulator's type.</typeparam>
/// <typeparam name="TConfig">The type for the format's config.</typeparam>
public class EmulateComponent<TEmulator, TConfig> : IEmulateComponent
    where TEmulator : Emulator<TConfig>
    where TConfig : EmulatorConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmulateComponent{TEmulator, TConfig}"/> class.
    /// </summary>
    public EmulateComponent(TConfig config, IEmulatorDescriptor descriptor)
    {
        Config = config;
        Descriptor = descriptor;
    }

    /// <inheritdoc/>
    public TConfig Config { get; }

    private IEmulatorDescriptor Descriptor { get; }

    EmulatorConfig IEmulateComponent.Config => Config;

    /// <inheritdoc/>
    public IEmulator? CreateEmulator() => Descriptor.Create(Config);
}
