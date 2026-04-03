// Avishai Dernis 2026

using System.Numerics;
using Zarem.Emulator.Models.Enum;
using Zarem.Models.Instructions;

namespace Zarem.Emulator.Models;

/// <summary>
/// A struct which handles converting decoded instructions into <see cref="MipsExecution{T}"/> models.
/// </summary>
public abstract partial class InstructionServiceTable<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="instruction"></param>
    /// <param name="execution"></param>
    /// <returns></returns>
    public abstract MipsTrap Execute(MipsInstruction instruction, out MipsExecution<T> execution);
}
