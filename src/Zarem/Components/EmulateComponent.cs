// Avishai Dernis 2026

using Zarem.Components.Interfaces;
using Zarem.Descriptors;
using Zarem.Emulator;
using Zarem.Emulator.Config;
using Zarem.Emulator.Machine.Interfaces;

namespace Zarem.Components;

/// <summary>
/// A component of a <see cref="Project"/> that emulates machines.
/// </summary>
/// <typeparam name="TComputer">The type of the computer created.</typeparam>
/// <typeparam name="TConfig">The type for the format's config.</typeparam>
public class EmulateComponent<TComputer, TConfig> : IEmulateComponent
    where TComputer : IComputer
    where TConfig : EmulatorConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmulateComponent{TComputer, TConfig}"/> class.
    /// </summary>
    public EmulateComponent(TConfig config, IComputerDescriptor descriptor)
    {
        Config = config;
        Descriptor = descriptor;
    }

    /// <inheritdoc/>
    public TConfig Config { get; }

    private IComputerDescriptor Descriptor { get; }

    EmulatorConfig IEmulateComponent.Config => Config;

    /// <inheritdoc/>
    public Zaremulator? CreateEmulator()
    {
        var computer = Descriptor.Create(Config);
        if (computer is null)
            return null;

        return new Zaremulator(computer);
    }
}
