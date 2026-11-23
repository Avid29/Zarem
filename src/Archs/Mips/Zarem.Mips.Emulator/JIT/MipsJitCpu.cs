// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.CompilerServices;
using Zarem.Emulator.JIT;
using Zarem.Emulator.Machine.Memory;
using Zarem.Emulator.Models.JIT;
using Zarem.Mips.Emulator.Config;
using Zarem.Mips.Emulator.JIT.CoProcessors;
using Zarem.Mips.Emulator.JIT.CoProcessors.FloatCoProcessor;
using Zarem.Mips.Emulator.Machine;
using Zarem.Mips.Emulator.Machine.CoProcessors;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Versioning.Enums;

namespace Zarem.Mips.Emulator.JIT;

/// <summary>
/// A <see cref="MipsCpu{T}"/> which uses JIT cross-compilation for execution.
/// </summary>
public partial class MipsJitCpu<T, TFloat> : MipsCpu<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>
{
    // Cache mapping a PC to its compiled IL block.
    private readonly JitBlockCache<T, MipsJitBlock<T, TFloat>> _blockCache;
    private readonly MipsJitCompiler<T, TFloat> _jitCompiler;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsCpu{T}"/> class.
    /// </summary>
    public MipsJitCpu(MipsEmulatorConfig config, PhysicalBus bus) : base(config, bus)
    {
        _blockCache = new();
        _jitCompiler = new MipsJitCompiler<T, TFloat>(this);

        var coProcessorCount = config.VersionInfo.Generation is < MipsGeneration.MipsIII ? 3 : 2;
        CoProcessors = new IJitCoProcessor<T>[coProcessorCount];

        FloatCoProcessor = new JitFloatCoProcessor<T, TFloat>(ControlCoProcessor);
        CoProcessors[1] = FloatCoProcessor;

        bus.AddressWritten += OnAddressWritten;
    }

    /// <inheritdoc/>
    public override IJitCoProcessor<T>[] CoProcessors { get; }

    /// <inheritdoc/>
    public override JitFloatCoProcessor<T, TFloat> FloatCoProcessor { get; }

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
        return long.CreateTruncating(compiledBlock.Size);
    }

    private void OnAddressWritten(object? sender, ulong e)
    {
        // Invalidate the JIT cache for the page that was modified.
        // This allows self-modifying code to work correctly, as the next time the CPU tries to execute
        // from that address, it will recompile the block.
        _blockCache.InvalidatePage(T.CreateTruncating(e));
    }
}
