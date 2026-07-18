// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.CompilerServices;
using Zarem.Emulator.JIT;
using Zarem.Emulator.Machine.Memory;
using Zarem.RiscV.Emulator.Config;
using Zarem.RiscV.Emulator.Machine;
using Zarem.RiscV.Emulator.Machine.Enums;
using Zarem.RiscV.Models.Instructions;

namespace Zarem.RiscV.Emulator.JIT;

/// <summary>
/// A <see cref="RiscVJitCpu{T, TFloat}"/> which uses JIT cross-compilation for execution.
/// </summary>
public class RiscVJitCpu<T, TFloat> : RiscVCpu<T, TFloat>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>
{
    private readonly JitBlockCache<T, RiscVJitBlock<T, TFloat>> _blockCache;
    private readonly RiscVJitCompiler<T, TFloat> _jitCompiler;

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVJitCpu{T, TFloat}"/> class.
    /// </summary>
    public RiscVJitCpu(RiscVEmulatorConfig config, PhysicalBus bus) : base(config, bus)
    {
        _blockCache = new();
        _jitCompiler = new RiscVJitCompiler<T, TFloat>(this);

        bus.AddressWritten += OnAddressWritten;
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
        return long.CreateTruncating(compiledBlock.Size);
    }

    private void OnAddressWritten(object? sender, ulong e)
    {
        // Invalidate the JIT cache for the page that was modified.
        // This allows self-modifying code to work correctly, as the next time the CPU tries to execute
        // from that address, it will recompile the block.
        _blockCache.InvalidateBlock(T.CreateTruncating(e));
    }
}
