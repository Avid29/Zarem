// Avishai Dernis 2026

using System.Numerics;
using System.Reflection.Emit;
using Zarem.Mips.Emulator.Machine.CoProcessors;
using Zarem.Mips.Models.Instructions;

namespace Zarem.Mips.Emulator.JIT.CoProcessors.FloatCoProcessor;

/// <summary>
/// A <see cref="FloatCoProcessor{T, TFloat}"/> that executes by JIT cross-compilation.
/// </summary>
public class JitFloatCoProcessor<T, TFloat> : FloatCoProcessor<T, TFloat>, IJitFloatCoProcessor<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>
{
    private readonly MipsFloatJitCompiler<T, TFloat> _jitCompiler;

    /// <summary>
    /// Initializes a new instance of the <see cref="JitFloatCoProcessor{T, TFloat}"/> class.
    /// </summary>
    public JitFloatCoProcessor(MipsJitCpu<T> cpu) : base(cpu.ControlCoProcessor)
    {
        _jitCompiler = new MipsFloatJitCompiler<T, TFloat>(cpu, this);
    }

    /// <inheritdoc/>
    public void CompileInstruction(ILGenerator il, MipsInstruction instruction, T pc)
    {
        _jitCompiler.CompileInstruction(il, instruction, pc);
    }
}
