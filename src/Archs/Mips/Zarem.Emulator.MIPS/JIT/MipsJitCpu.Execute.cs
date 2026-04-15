// Avishai Dernis 2026

using System.Numerics;
using System.Threading;
using Zarem.Emulator.Machine;

namespace Zarem.Emulator.JIT;

/// <summary>
/// Represents a compiled block of MIPS instructions.
/// </summary>
/// <typeparam name="T">The register width (uint or ulong).</typeparam>
/// <param name="cpu">The CPU instance to operate on.</param>
/// <returns>The Program Counter where execution should continue.</returns>
public delegate T MipsBlockDelegate<T>(MipsJitCpu<T> cpu)
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>;

public partial class MipsJitCpu<T> : IMipsCpu
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    // Cache mapping a PC to its compiled IL block.
    private readonly MipsBlockCache<T> _blockCache = new();
    private readonly MipsJitCompiler<T> _jitCompiler;

    /// <inheritdoc/>
    public void Run(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            // Look up the current block
            if (!_blockCache.TryGet(ProgramCounter, out var compiledBlock))
            {
                // Cache Miss. Compile the basic block starting at ProgramCounter
                compiledBlock = _jitCompiler.CompileBlock(ProgramCounter);
                _blockCache.Store(ProgramCounter, compiledBlock);
            }

            // Execute the block, and update the PC to the next block start
            ProgramCounter = compiledBlock(this);
        }
    }
}
