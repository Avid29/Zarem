// Avishai Dernis 2026

using System.Numerics;

namespace Zarem.Emulator.Machine.Registers;

/// <summary>
/// An interface for indexing registers with different formats.
/// </summary>
/// <typeparam name="T">The indexer's format.</typeparam>
public interface IFormattedRegisterIndexer<T>
    where T : INumber<T>
{
    /// <summary>
    /// Gets or sets the value of a register on the coprocessor as a <typeparamref name="T"/>.
    /// </summary>
    /// <param name="reg">The register to get or set.</param>
    /// <returns>The value of the register.</returns>
    T this[int reg] { get; set; }
}
