// Avishai Dernis 2026

using System.Numerics;
using System.Reflection.Emit;
using System.Threading;
using Zarem.Emulator.Config;
using Zarem.Emulator.Events;
using Zarem.Emulator.JIT;
using Zarem.Emulator.Models.Enums;
using Zarem.Emulator.Models.JIT;
using Zarem.Emulator.TrapHandlers;
using Zarem.Models.Instructions;

namespace Zarem.Emulator.Machine.JIT;

/// <summary>
/// Represents a compiled block of MIPS instructions.
/// </summary>
/// <typeparam name="T">The register width (uint or ulong).</typeparam>
/// <param name="cpu">The CPU instance to operate on.</param>
/// <returns>The Program Counter where execution should continue.</returns>
public delegate T MipsBlockDelegate<T>(MipsJitCpu<T> cpu)
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>;

/// <summary>
/// A <see cref="MipsCpu{T}"/> which uses JIT cross-compilation for execution.
/// </summary>
public partial class MipsJitCpu<T> : MipsCpu<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    // Cache mapping a PC to its compiled IL block.
    private readonly MipsBlockCache<T> _blockCache;
    private readonly MipsJitCompiler<T> _jitCompiler;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsCpu{T}"/> class.
    /// </summary>
    public MipsJitCpu(MipsEmulatorConfig config, PhysicalBus bus) : base(config, bus)
    {
        _blockCache = new();
        _jitCompiler = new MipsJitCompiler<T>(this);
    }

    /// <inheritdoc/>
    public override void Insert(MipsInstruction instruction, out MipsTrap trap)
    {
        trap = MipsTrap.None;
        var @delegate = _jitCompiler.CompileLoneInstruction(instruction, ProgramCounter);
        ProgramCounter = @delegate(this);
    }

    /// <inheritdoc/>
    public override void Run(CancellationToken ct)
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

    /// <summary>
    /// Handles a trap.
    /// </summary>
    public void HandleTrap(int trapCode, T currentPc)
    {
        var trap = (MipsTrap)trapCode;

        // Sync the PC so the interpreter/debugger knows where we are
        ProgramCounter = currentPc;

        base.HandleTrap(trap);
    }
}
