// Avishai Dernis 2026

using System;
using System.Numerics;
using System.Reflection.Emit;
using Zarem.Emulator.Machine.CPU;

namespace Zarem.Emulator.JIT.Compilers;

/// <summary>
/// A base class for a component which recompiles blocks of assembly into .NET CIL code.
/// </summary>
public abstract partial class JitCompiler<T, TBlock, TRegister, TTrap> : JitCompiler<T, TRegister, TTrap>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TBlock : JitBlock<T>
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
    /// Compiles a block of RISC-V code to JIT starting at <paramref name="startPc"/>.
    /// </summary>
    /// <param name="startPc">The entry point of the JIT block.</param>
    /// <returns>The method block.</returns>
    public override TBlock CompileBlock(T startPc)
    {
        Type[] parameterTypes = [Cpu.GetType(), typeof(TRegister).MakeByRefType()];
        var method = new DynamicMethod($"Block_0x{startPc:X}", typeof(T), parameterTypes, true);
        var il = method.GetILGenerator();
        var context = new JitBlockContext<T, TRegister, TTrap>(il, startPc);

        T currentPc = startPc;
        while (CompileNextInstruction(context, ref currentPc));

        context.EmitSetupLocalRegisters(this);
        return CreateBlockDelegate(method, context);
    }

    /// <summary>
    /// Creates a <see cref="JitBlock{T}"/> from a method and <see cref="JitBlockContext{T, TRegister, TTrap}"/>.
    /// </summary>
    /// <param name="method">The method generated.</param>
    /// <param name="context">The <see cref="JitBlockContext{T, TRegister, TTrap}"/>.</param>
    /// <returns>A new <see cref="JitBlock{T}"/>.</returns>
    protected abstract TBlock CreateBlockDelegate(DynamicMethod method, JitBlockContext<T, TRegister, TTrap> context);

}
