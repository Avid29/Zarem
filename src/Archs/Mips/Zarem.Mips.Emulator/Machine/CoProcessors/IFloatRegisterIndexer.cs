// Avishai Dernis 2026

using System.Numerics;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.Machine.CoProcessors;

/// <summary>
/// An interface for indexing the FPU registers with different formats.
/// </summary>
/// <typeparam name="T">The indexer's format.</typeparam>
public interface IFloatRegisterIndexer<T>
    where T : INumber<T>
{
    /// <summary>
    /// Gets or sets the value of a register on the coprocessor as a <typeparamref name="T"/>.
    /// </summary>
    /// <param name="reg">The register to get or set.</param>
    /// <returns>The value of the register.</returns>
    T this[MipsFloatRegister reg] { get; set; }
}
