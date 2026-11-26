// Avishai Dernis 2024

using System.Numerics;
using Zarem.Emulator.JIT;
using Zarem.Emulator.JIT.Compilers;
using Zarem.Emulator.Machine.CoProcessors;
using Zarem.Mips.Emulator.Interpret;
using Zarem.Mips.Emulator.Interpret.CoProcessors.Control;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Mips.Emulator.Machine.CoProcessors.Control;

/// <summary>
/// A class representing the status/control coprocessor unit.
/// </summary>
public partial class ControlCoProcessor<T> : IMipsCoProcessor<T>, IControlCoProcessor
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <summary>
    /// An <see cref="ICoProcessor{T}.ICoExecutor{TInstruction,TExecution,TRegister, TTrap}"/> for an interpreted MIPS co0.
    /// </summary>
    protected class ControlInterpretedExecutor : ICoProcessor<T>.ICoExecutor<MipsInstruction, MipsExecution<T>, MipsGpRegister, MipsTrap>
    {
        private readonly MipsControlInstructionServiceTable<T> _instructionServiceTable;
        private readonly ControlCoProcessor<T> _c0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlInterpretedExecutor"/> class.
        /// </summary>
        public ControlInterpretedExecutor(MipsCpu<T> cpu, ControlCoProcessor<T> c0)
        {
            _instructionServiceTable = new MipsControlInstructionServiceTable<T>(cpu, c0);
            _c0 = c0;
        }

        /// <inheritdoc/>
        public JitChildCompiler<T>? JitChildCompiler => null;

        /// <inheritdoc/>
        public bool CompileInstruction(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction instruction, T pc)
            => throw new System.NotImplementedException();

        /// <inheritdoc/>
        public MipsTrap Execute(MipsInstruction instruction, out MipsExecution<T> execution)
        {
            var trap = _instructionServiceTable.Execute(instruction, out var fExecution);
            execution = fExecution.ChildExecution;

            if (trap is not MipsTrap.None)
                return trap;

            switch (fExecution.TlbOperation)
            {
                case TlbOperation.TLBProbe:
                    _c0.WritebackTlbp();
                    break;
                case TlbOperation.TLBRead:
                    _c0.WritebackTlbr();
                    break;
                case TlbOperation.TLBWriteIndexed:
                    _c0.WritebackTlbwi();
                    break;
                case TlbOperation.TLBWriteRandom:
                    _c0.WritebackTlbwr();
                    break;
            }

            return MipsTrap.None;
        }
    }
}
