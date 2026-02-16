// Avishai Dernis 2026

using System;
using Zarem.Emulator;

namespace Zarem.Descriptors;

/// <summary>
/// An interface for a class describing an emulator.
/// </summary>
public interface IEmulatorDescriptor : IDescriptor
{
    /// <summary>
    /// Gets the <see cref="Type"/> of the emulator.
    /// </summary>
    Type EmulatorType { get; }

    /// <summary>
    /// Creates a new emulator using the provided config.
    /// </summary>
    /// <param name="config">The emulator config.</param>
    /// <returns>A new emulator.</returns>
    IEmulator? Create(object config);
}
