// Avishai Dernis 2026

using System.Numerics;

namespace Zarem.Emulator.Machine.Registers;

/// <summary>
/// An interface for a register file with different indexable formats.
/// </summary>
public interface IFormattedRegisterFile<T> : IRegisterFile<T>, IFormattedRegisterFile
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
}
