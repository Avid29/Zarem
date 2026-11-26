// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Numerics;
using Zarem.Emulator.JIT;
using Zarem.Emulator.JIT.Compilers;
using Zarem.Emulator.Machine.CoProcessors;
using Zarem.Emulator.Models.JIT;
using Zarem.Mips.Emulator.Interpret;
using Zarem.Mips.Emulator.JIT.CoProcessors.FloatCoProcessor;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Mips.Emulator.Machine.CoProcessors.FloatingPoint;

/// <summary>
/// a class representing the floating-point coprocessor unit.
/// </summary>
public partial class FloatCoProcessor<T, TFloat> : IFloatCoProcessor<T>, IDisposable
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>
{
    /// <summary>
    /// An <see cref="ICoProcessor{T}.ICoExecutor{TInstruction,TExecution,TRegister,TTrap}"/> for an JIT cross-compiling MIPS fpu.
    /// </summary>
    protected class FloatJitExecutor : ICoProcessor<T>.ICoExecutor<MipsInstruction, MipsExecution<T>, MipsGpRegister, MipsTrap>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FloatJitExecutor"/> class.
        /// </summary>
        public FloatJitExecutor(MipsCpu<T> cpu, FloatCoProcessor<T, TFloat> fpu)
        {
            if (cpu.Executor.JitCompiler is MipsJitCompiler<T> parent)
            {
                JitChildCompiler = new MipsFloatJitCompiler<T, TFloat>(parent, fpu);
                return;
            }

            ThrowHelper.ThrowArgumentException();
        }

        /// <inheritdoc/>
        public MipsFloatJitCompiler<T, TFloat> JitChildCompiler { get; }

        /// <inheritdoc/>
        JitChildCompiler<T>? ICoProcessor<T>.ICoExecutor<MipsInstruction, MipsExecution<T>, MipsGpRegister, MipsTrap>.JitChildCompiler => JitChildCompiler;

        /// <inheritdoc/>
        public bool CompileInstruction(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction instruction, T pc)
            => JitChildCompiler.CompileInstruction(context, instruction, pc);

        /// <inheritdoc/>
        public MipsTrap Execute(MipsInstruction instruction, out MipsExecution<T> execution)
            => throw new NotImplementedException();
    }
}
