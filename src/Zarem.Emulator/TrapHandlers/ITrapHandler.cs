// Avishai Dernis 2026

using Zarem.Emulator.Machine.Interfaces;

namespace Zarem.Emulator.TrapHandlers;

/// <summary>
/// A base class for a class that handles traps on behalf of the emulator.
/// </summary>
public interface ITrapHandler
{
    /// <summary>
    /// Defers trap handling to a custom implementation.
    /// </summary>
    /// <param name="computer">The computer instance (can be cast to specific architecture types).</param>
    /// <param name="trapCode">The raw architecture-specific trap or exception code.</param>
    void HandleTrap(IComputer computer, ulong trapCode);
}
