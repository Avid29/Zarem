// Avishai Dernis 2026

using System;
using Zarem.Emulator.JIT.Compilers;

namespace Zarem.Emulator.Machine.CPU;

/// <summary>
/// A base class for an emulated CPU.
/// </summary>
public abstract partial class CpuBase<T>
{
    /// <summary>
    /// An interface for handling CPU execution.
    /// </summary>
    public interface IExecutor<TInstruction, TTrap>
        where TTrap : Enum
    {
        /// <summary>
        /// Gets the <see cref="JitCompiler{T}"/> used by the executor, if any.
        /// </summary>
        public JitCompiler<T>? JitCompiler { get; }

        /// <summary>
        /// The base dispatch of the execution loop.
        /// </summary>
        /// <remarks>
        /// This method itself does NOT loop.
        /// </remarks>
        /// <returns>The number of instructions executed in the loop step.</returns>
        long ExecutionLoop();

        /// <summary>
        /// Executes an instruction on the current state of the processor.
        /// </summary>
        public void Insert(TInstruction instruction, out TTrap trap);
    }
}
