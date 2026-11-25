// Avishai Dernis 2025

using System.Runtime.CompilerServices;
using Zarem.Emulator.JIT;
using Zarem.Emulator.JIT.Compilers;
using Zarem.Emulator.Machine.CPU;
using Zarem.Emulator.Machine.Memory;
using Zarem.Emulator.Models.JIT;
using Zarem.Mips.Emulator.JIT;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models.Instructions;

namespace Zarem.Mips.Emulator.Machine;

/// <summary>
/// A base class representing a processor unit.
/// </summary>
public partial class MipsCpu<T>
{
    /// <summary>
    /// A <see cref="CpuBase{T}.IExecutor{TInstruction,TTrap}"/> for a JIT cross-compiling MIPS cpu.
    /// </summary>
    protected class MipsJitExecutor : IExecutor<MipsInstruction, MipsTrap>
    {
        // Cache mapping a PC to its compiled IL block.
        private readonly JitBlockCache<T, MipsJitBlock<T>> _blockCache;
        private readonly MipsCpu<T> _cpu;

        /// <summary>
        /// Initializes a new instance of the <see cref="MipsJitExecutor"/> class.
        /// </summary>
        public MipsJitExecutor(MipsCpu<T> cpu, PhysicalBus bus)
        {
            _cpu = cpu;

            _blockCache = new();
            JitCompiler = new MipsJitCompiler<T>(cpu);

            bus.AddressWritten += OnAddressWritten;
        }

        /// <inheritdoc/>
        public MipsJitCompiler<T> JitCompiler { get; }

        /// <inheritdoc/>
        JitCompiler<T> IExecutor<MipsInstruction, MipsTrap>.JitCompiler => JitCompiler;

        /// <inheritdoc/>
        public void Insert(MipsInstruction instruction, out MipsTrap trap)
        {
            var @delegate = JitCompiler.CompileLoneInstruction(instruction, _cpu.ProgramCounter);
            _cpu.ProgramCounter = @delegate(_cpu, out trap);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ExecutionLoop()
        {
            // Look up the current block
            if (!_blockCache.TryGet(_cpu.ProgramCounter, out var compiledBlock))
            {
                // Cache Miss. Compile the basic block starting at ProgramCounter
                compiledBlock = JitCompiler.CompileBlock(_cpu.ProgramCounter);
                _blockCache.Store(_cpu.ProgramCounter, compiledBlock);
            }

            // Execute the block, and update the PC to the next block start
            _cpu.ProgramCounter = compiledBlock.Delegate(_cpu, out var trap);

            // Handle trap
            if (trap is not MipsTrap.None)
                _cpu.HandleTrap(trap);

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
}
