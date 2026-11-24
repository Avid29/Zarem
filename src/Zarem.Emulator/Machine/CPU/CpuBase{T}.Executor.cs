// Avishai Dernis 2026

using System;

namespace Zarem.Emulator.Machine.CPU;

/// <summary>
/// A base class for an emulated CPU.
/// </summary>
public abstract partial class CpuBase<T>
{
    /// <summary>
    /// An interface for handling CPU execution.
    /// </summary>
    protected interface IExecutor<TInstruction, TTrap>
        where TTrap : Enum
    {
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
