// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Emulator.Machine.CPU;

namespace Zarem.Emulator.JIT.Compilers;

/// <summary>
/// A base class for a component which recompiles blocks of assembly into .NET CIL code.
/// </summary>
public abstract partial class JitCompiler<T, TRegister, TTrap> : JitCompiler<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TRegister : unmanaged, Enum
    where TTrap : unmanaged, Enum
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JitCompiler{T, TBlock, TRegister, TTrap}"/> class.
    /// </summary>
    public JitCompiler(ICpu cpu) : base(cpu)
    {
    }

    /// <summary>
    /// Compiles an instruction.
    /// </summary>
    protected abstract bool CompileNextInstruction(JitBlockContext<T, TRegister, TTrap> context, ref T pc);

}
