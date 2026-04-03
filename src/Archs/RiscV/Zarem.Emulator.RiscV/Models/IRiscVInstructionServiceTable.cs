// Avishai Dernis 2026

using System.Numerics;
using Zarem.Emulator.Models.Enums;
using Zarem.Models.Instructions;

namespace Zarem.Emulator.Models;

/// <summary>
/// A struct which handles converting decoded instructions into <see cref="RiscVExecution{T}"/> models.
/// </summary>
public interface IRiscVInstructionServiceTable<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <summary>
    /// Creates a <see cref="RiscVExecution{T}{T}"/> model for a given <see cref="RiscVInstruction"/>.
    /// </summary>
    RiscVTrap Execute(RiscVInstruction instruction, out RiscVExecution<T> execution);
}
