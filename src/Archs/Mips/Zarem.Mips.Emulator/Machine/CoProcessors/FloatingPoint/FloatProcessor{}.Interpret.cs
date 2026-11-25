// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Emulator.JIT;
using Zarem.Emulator.JIT.Compilers;
using Zarem.Emulator.Machine.CoProcessors;
using Zarem.Mips.Emulator.Interpret;
using Zarem.Mips.Emulator.Interpret.CoProcessors;
using Zarem.Mips.Emulator.Interpret.CoProcessors.FloatCoProcessor;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Mips.Emulator.Machine.CoProcessors.FloatingPoint;

/// <summary>
/// a class representing the floating-point coprocessor unit.
/// </summary>
public partial class FloatCoProcessor<T, TFloat> : IFloatCoProcessor<T>, IDisposable
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>
{
    /// <summary>
    /// An <see cref="ICoProcessor{T}.ICoExecutor{TInstruction,TExecution,TRegister, TTrap}"/> for an interpreted MIPS fpu.
    /// </summary>
    protected class FloatInterpretedExecutor : ICoProcessor<T>.ICoExecutor<MipsInstruction, MipsExecution<T>, MipsGpRegister, MipsTrap>
    {
        private readonly MipsFloatInstructionServiceTable<T, TFloat> _instructionServiceTable;
        private readonly FloatCoProcessor<T, TFloat> _fpu;

        /// <summary>
        /// Initializes a new instance of the <see cref="FloatInterpretedExecutor"/> class.
        /// </summary>
        public FloatInterpretedExecutor(MipsCpu<T> cpu, FloatCoProcessor<T, TFloat> fpu)
        {
            _instructionServiceTable = new MipsFloatInstructionServiceTable<T, TFloat>(cpu, fpu);
            _fpu = fpu;
        }

        /// <inheritdoc/>
        public JitChildCompiler<T>? JitChildCompiler => null;

        /// <inheritdoc/>
        public void CompileInstruction(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction instruction, T pc)
            => throw new NotImplementedException();

        /// <inheritdoc/>
        public MipsTrap Execute(MipsInstruction instruction, out MipsExecution<T> execution)
        {
            var trap = _instructionServiceTable.Execute(instruction, out var fExecution);
            execution = fExecution.ChildExecution;

            if (trap is not MipsTrap.None)
                return trap;

            switch (fExecution.Format)
            {
                case FloatWriteback.Word:
                    _fpu.Words[(int)fExecution.Register] = (int)fExecution.Writeback;
                    break;
                case FloatWriteback.Long:
                    _fpu.Longs[(int)fExecution.Register] = fExecution.Writeback;
                    break;
                case FloatWriteback.Control:
                    _fpu.ControlRegisterFile[(int)fExecution.Register] = (uint)fExecution.Writeback;
                    break;
            }

            return MipsTrap.None;
        }
    }
}
