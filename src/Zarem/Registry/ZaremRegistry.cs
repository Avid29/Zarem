// Avishai Dernis 2026

using Zarem.Descriptors;

namespace Zarem.Registry;

/// <summary>
/// A static location for registered zarem components.
/// </summary>
public static class ZaremRegistry
{
    /// <summary>
    /// Gets the architecture registry.
    /// </summary>
    public static DescriptorRegistry<IArchitectureDescriptor> Architectures { get; } = new();

    /// <summary>
    /// Gets the architecture registry.
    /// </summary>
    public static DescriptorRegistry<IAssemblerDescriptor> Assemblers { get; } = new();

    /// <summary>
    /// Gets the architecture registry.
    /// </summary>
    public static DescriptorRegistry<IEmulatorDescriptor> Emulators { get; } = new();

    /// <summary>
    /// Gets the architecture registry.
    /// </summary>
    public static DescriptorRegistry<IModuleFormatDescriptor> Formats { get; } = new();

    /// <summary>
    /// Registers an architecture and its assembler and emulator.
    /// </summary>
    public static void RegisterArchitecture(IArchitectureDescriptor descriptor)
    {
        Architectures.Register(descriptor);
        Assemblers.Register(descriptor.Assembler);
        Emulators.Register(descriptor.Emulator);
    }
}
