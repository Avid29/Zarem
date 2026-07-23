// Avishai Dernis 2026

using System.Numerics;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models.Instructions;

namespace Zarem.Mips.Emulator.Interpret;

/// <summary>
/// A struct which handles converting decoded instructions into <see cref="MipsExecution{T}"/> models.
/// </summary>
public interface IMipsInstructionServiceTable<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <summary>
    /// Creates a <see cref="MipsExecution{T}"/> model for a given <see cref="MipsInstruction"/>.
    /// </summary>
    MipsTrap Execute(MipsInstruction instruction, out MipsExecution<T> execution);
}
