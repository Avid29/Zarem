// Avishai Dernis 2026

using System;
using Zarem.Descriptors.Base;
using Zarem.Emulator.Machine;

namespace Zarem.Descriptors;

/// <summary>
/// An interface for a class describing an emulated computer.
/// </summary>
public interface IComputerDescriptor : IConfigDescriptor
{
    /// <summary>
    /// Gets the <see cref="Type"/> of the emulator.
    /// </summary>
    Type ComputerType { get; }

    /// <summary>
    /// Creates a new computer using the provided config.
    /// </summary>
    /// <param name="config">The emulator config.</param>
    /// <returns>A new <see cref="IComputer"/>.</returns>
    IComputer? Create(object config);
}
