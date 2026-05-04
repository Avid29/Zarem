// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.CompilerServices;
using Zarem.Emulator.JIT;
using Zarem.Emulator.Machine;
using Zarem.RiscV.Emulator.Config;
using Zarem.RiscV.Emulator.Machine;
using Zarem.RiscV.Emulator.Machine.Enums;
using Zarem.RiscV.Models.Instructions;

namespace Zarem.RiscV.Emulator.JIT;

/// <summary>
/// A <see cref="RiscVJitCpu{T}"/> which uses JIT cross-compilation for execution.
/// </summary>
public class RiscVJitCpu<T> : RiscVCpu<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private readonly JitBlockCache<T, RiscVJitBlock<T>> _blockCache;
    private readonly RiscVJitCompiler<T> _jitCompiler;

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVCpu{T}"/> class.
    /// </summary>
    public RiscVJitCpu(RiscVEmulatorConfig config, PhysicalBus bus) : base(config, bus)
    {
        _blockCache = new();
        _jitCompiler = new RiscVJitCompiler<T>(this);
    }

    /// <inheritdoc/>
    public override void Insert(RiscVInstruction instruction, out RiscVTrap trap)
    {
        var @delegate = _jitCompiler.CompileLoneInstruction(instruction, ProgramCounter);
        ProgramCounter = @delegate(this, out trap);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override long ExecutionLoop()
    {
        // Look up the current block
        if (!_blockCache.TryGet(ProgramCounter, out var compiledBlock))
        {
            // Cache Miss. Compile the basic block starting at ProgramCounter
            compiledBlock = _jitCompiler.CompileBlock(ProgramCounter);
            _blockCache.Store(ProgramCounter, compiledBlock);
        }

        // Execute the block, and update the PC to the next block start
        ProgramCounter = compiledBlock.Delegate(this, out var trap);

        // Handle trap
        if (trap is not RiscVTrap.None)
            HandleTrap(trap);

        // Return the number of instructions executed.
        return compiledBlock.Size;
    }
}
