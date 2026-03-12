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
    /// Gets the assembler registry.
    /// </summary>
    public static DescriptorRegistry<IAssemblerDescriptor> Assemblers { get; } = new();

    /// <summary>
    /// Gets the linker registry.
    /// </summary>
    public static DescriptorRegistry<ILinkerDescriptor> Linkers { get; } = new();

    /// <summary>
    /// Gets the emulator registry.
    /// </summary>
    public static DescriptorRegistry<IComputerDescriptor> Emulators { get; } = new();

    /// <summary>
    /// Gets the format registry.
    /// </summary>
    public static DescriptorRegistry<IModuleFormatDescriptor> Formats { get; } = new();

    /// <summary>
    /// Registers an architecture and its assembler and emulator.
    /// </summary>
    public static void RegisterArchitecture(IArchitectureDescriptor descriptor)
    {
        Architectures.Register(descriptor);
        Assemblers.Register(descriptor.Assembler);
        Emulators.Register(descriptor.Computer);
        Linkers.Register(descriptor.Linker);
    }
}
