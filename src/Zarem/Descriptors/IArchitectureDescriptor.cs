// Avishai Dernis 2026

using Zarem.Descriptors.Base;

namespace Zarem.Descriptors;

/// <summary>
/// An interface for a class describing a supported architecture.
/// </summary>
public interface IArchitectureDescriptor : IConfigDescriptor, IDisplayDescriptor
{
    /// <summary>
    /// Gets the <see cref="IAssemblerDescriptor"/> for the architecture's assembler.
    /// </summary>
    IAssemblerDescriptor Assembler { get; }

    /// <summary>
    /// Gets the <see cref="ILinkerDescriptor"/> for the architecture's linker.
    /// </summary>
    ILinkerDescriptor Linker { get; }

    /// <summary>
    /// Gets the <see cref="IComputerDescriptor"/> for the architecture's emulated computer.
    /// </summary>
    IComputerDescriptor Computer { get; }

    /// <summary>
    /// Gets the <see cref="IDebuggerDescriptor"/> for the architecture's emulated computer.
    /// </summary>
    IDebuggerDescriptor Debugger { get; }
}
