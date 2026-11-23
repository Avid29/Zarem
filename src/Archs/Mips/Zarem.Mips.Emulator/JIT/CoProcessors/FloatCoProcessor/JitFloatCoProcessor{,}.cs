// Avishai Dernis 2026

using System.Numerics;
using System.Reflection.Emit;
using Zarem.Mips.Emulator.Machine.CoProcessors;
using Zarem.Mips.Models.Instructions;

namespace Zarem.Mips.Emulator.JIT.CoProcessors.FloatCoProcessor;

/// <summary>
/// A <see cref="FloatCoProcessor{T, TFloat}{T}"/> that executes by JIT cross-compilation.
/// </summary>
public class JitFloatCoProcessor<T, TFloat> : FloatCoProcessor<T, TFloat>, IJitCoProcessor<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JitFloatCoProcessor{T, TFloat}"/> class.
    /// </summary>
    public JitFloatCoProcessor(IControlCoProcessor co0) : base(co0)
    {
    }

    /// <inheritdoc/>
    public void CompileInstruction(ILGenerator il, MipsInstruction instruction, T pc)
    {
        throw new System.NotImplementedException();
    }
}
