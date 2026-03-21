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
    public static ConfigDescriptorRegistry<IArchitectureDescriptor> Architectures { get; } = new();

    /// <summary>
    /// Gets the assembler registry.
    /// </summary>
    public static ConfigDescriptorRegistry<IAssemblerDescriptor> Assemblers { get; } = new();

    /// <summary>
    /// Gets the linker registry.
    /// </summary>
    public static ConfigDescriptorRegistry<ILinkerDescriptor> Linkers { get; } = new();

    /// <summary>
    /// Gets the emulator registry.
    /// </summary>
    public static ConfigDescriptorRegistry<IComputerDescriptor> Emulators { get; } = new();

    /// <summary>
    /// Gets the format registry.
    /// </summary>
    public static ConfigDescriptorRegistry<IModuleFormatDescriptor> Formats { get; } = new();

    /// <summary>
    /// Gets the trap handler registry.
    /// </summary>
    public static TypeDescriptorRegistry<ITrapHandlerDescriptor> TrapHandlers { get; } = new();

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
