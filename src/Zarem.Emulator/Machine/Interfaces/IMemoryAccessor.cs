// Avishai Dernis 2026

namespace Zarem.Emulator.Machine.Interfaces;

/// <summary>
/// An interface for RAM in a computer.
/// </summary>
public interface IMemoryAccessor
{
    /// <summary>
    /// Reads a <typeparamref name="T"/> from memory.
    /// </summary>
    /// <typeparam name="T">The type of object to read from memory.</typeparam>
    /// <param name="address">The address to read from.</param>
    /// <returns>The <typeparamref name="T"/> at <paramref name="address"/>.</returns>
    T Read<T>(ulong address) where T : unmanaged;

    /// <summary>
    /// Write a <typeparamref name="T"/> to memory.
    /// </summary>
    /// <typeparam name="T">The type of object to write from memory.</typeparam>
    /// <param name="address">The address to write to.</param>
    /// <param name="value">The value to write to memory.</param>
    void Write<T>(ulong address, T value) where T : unmanaged;
}
