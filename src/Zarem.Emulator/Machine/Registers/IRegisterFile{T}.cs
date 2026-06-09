// Avishai Dernis 2026

using System.Numerics;

namespace Zarem.Emulator.Machine.Registers;

/// <summary>
/// An interface for accessing an <see cref="RegisterFile{T}"/>.
/// </summary>
public unsafe interface IRegisterFile<T> : IRegisterFile
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <summary>
    /// Gets an unsafe point to the registers
    /// </summary>
    public T* Regs { get; }
}
