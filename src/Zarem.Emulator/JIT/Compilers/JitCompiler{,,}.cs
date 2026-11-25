// Avishai Dernis 2026

using System;
using System.Numerics;
using System.Reflection.Emit;
using Zarem.Emulator.JIT.Compilers;
using Zarem.Emulator.Machine.CPU;

namespace Zarem.Emulator.JIT;

/// <summary>
/// A base class for a component which recompiles blocks of assembly into .NET CIL code.
/// </summary>
public abstract partial class JitCompiler<T, TRegister, TTrap> : JitCompiler<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TRegister : unmanaged, Enum
    where TTrap : unmanaged, Enum
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JitCompiler{T, TRegister, TTrap}"/> class.
    /// </summary>
    public JitCompiler(ICpu cpu) : base(cpu)
    {
    }

    /// <summary>
    /// Creates a <see cref="JitBlock{T}"/> from a method and <see cref="JitBlockContext{T, TRegister, TTrap}"/>.
    /// </summary>
    /// <param name="method">The method generated.</param>
    /// <param name="context">The <see cref="JitBlockContext{T, TRegister, TTrap}"/>.</param>
    /// <returns>A new <see cref="JitBlock{T}"/>.</returns>
    protected abstract JitBlock<T> CreateBlockDelegate(DynamicMethod method, JitBlockContext<T, TRegister, TTrap> context);
}
