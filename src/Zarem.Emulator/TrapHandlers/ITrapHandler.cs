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
    /// <param name="cpu">The cput instance (can be cast to specific architecture types).</param>
    /// <param name="trapCode">The raw architecture-specific trap or exception code.</param>
    void HandleTrap(ICpu cpu, ulong trapCode);
}
