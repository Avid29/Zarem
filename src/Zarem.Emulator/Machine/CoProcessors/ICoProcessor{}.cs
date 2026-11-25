// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Emulator.JIT;
using Zarem.Emulator.JIT.Compilers;

namespace Zarem.Emulator.Machine.CoProcessors;

/// <summary>
/// An interface for a co-processor.
/// </summary>
public interface ICoProcessor<T> : ICoProcessor
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <summary>
    /// An interface for handling CPU execution.
    /// </summary>
    public interface ICoExecutor<TInstruction, TExecution, TRegister, TTrap>
        where TRegister : unmanaged, Enum
        where TTrap : unmanaged, Enum
    {
        /// <summary>
        /// Gets the <see cref="JitChildCompiler{T}"/> used by the executor, if any.
        /// </summary>
        public JitChildCompiler<T>? JitChildCompiler { get; }

        /// <summary>
        /// Executes a <typeparamref name="TInstruction"/>.
        /// </summary>
        /// <param name="instruction">The instruction to execute.</param>
        /// <param name="execution">The execution result.</param>
        /// <returns>The resulting trap if any.</returns>
        TTrap Execute(TInstruction instruction, out TExecution execution);

        /// <summary>
        /// Compiles an instruction 
        /// </summary>
        /// <param name="context">The <see cref="JitBlockContext{T}"/> to write to.</param>
        /// <param name="instruction">The instruction to cross compile.</param>
        /// <param name="pc">The address the instruction is at.</param>
        void CompileInstruction(JitBlockContext<T, TRegister, TTrap> context, TInstruction instruction, T pc);
    }
}
