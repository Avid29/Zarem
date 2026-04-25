// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Config;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Machine.Enums;
using Zarem.Emulator.Models.JIT;
using Zarem.Models.Instructions;

namespace Zarem.Emulator.JIT;

/// <summary>
/// A <see cref="MipsCpu{T}"/> which uses JIT cross-compilation for execution.
/// </summary>
public partial class MipsJitCpu<T> : MipsCpu<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    // Cache mapping a PC to its compiled IL block.
    private readonly JitBlockCache<T, MipsJitBlock<T>> _blockCache;
    private readonly MipsJitCompiler<T> _jitCompiler;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsCpu{T}"/> class.
    /// </summary>
    public MipsJitCpu(MipsEmulatorConfig config, PhysicalBus bus) : base(config, bus)
    {
        _blockCache = new();
        _jitCompiler = new MipsJitCompiler<T>(this);

        bus.AddressWritten += OnAddressWritten;
    }

    /// <inheritdoc/>
    public override void Insert(MipsInstruction instruction, out MipsTrap trap)
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
        if (trap is not MipsTrap.None)
            HandleTrap(trap);

        // Return the number of instructions executed.
        return compiledBlock.Size;
    }

    private void OnAddressWritten(object? sender, ulong e)
    {
        // TODO: Targeted block invalidation
        _blockCache.Clear();
    }
}
