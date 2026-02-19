// Avishai Dernis 2026

namespace Zarem.Descriptors;

/// <summary>
/// An interface for a class describing a supported architecture.
/// </summary>
public interface IArchitectureDescriptor : IDisplayDescriptor
{
    /// <summary>
    /// Gets the <see cref="IAssemblerDescriptor"/> for the architecture's assembler.
    /// </summary>
    IAssemblerDescriptor Assembler { get; }

    /// <summary>
    /// Gets the <see cref="ILinkerDescriptor"/> for the architecture's assembler.
    /// </summary>
    ILinkerDescriptor Linker { get; }

    /// <summary>
    /// Gets the <see cref="IEmulatorDescriptor"/> for the architecture's emulator.
    /// </summary>
    IEmulatorDescriptor Emulator { get; }
}
