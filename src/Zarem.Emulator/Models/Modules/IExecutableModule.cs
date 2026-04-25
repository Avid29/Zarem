// Avishai Dernis 2025

using Zarem.Models.Interface;

namespace Zarem.Emulator.Models.Modules;

/// <summary>
/// An interface representing an executable module in the emulator.
/// </summary>
public interface IExecutableModule : IModule
{
    /// <summary>
    /// Gets the entry address of the executable.
    /// </summary>
    public uint EntryAddress { get; }
}
