// Avishai Dernis 2024

using System.Numerics;
using Zarem.Emulator.JIT;
using Zarem.Emulator.JIT.Compilers;
using Zarem.Emulator.Machine.CoProcessors;
using Zarem.Mips.Emulator.Interpret;
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
    protected class ControlJitExecutor : ICoProcessor<T>.ICoExecutor<MipsInstruction, MipsExecution<T>, MipsGpRegister, MipsTrap>
    {
        /// <inheritdoc/>
        public JitChildCompiler<T>? JitChildCompiler => throw new System.NotImplementedException();

        /// <inheritdoc/>
        public void CompileInstruction(JitBlockContext<T, MipsGpRegister, MipsTrap> context, MipsInstruction instruction, T pc)
            => throw new System.NotImplementedException();

        /// <inheritdoc/>
        public MipsTrap Execute(MipsInstruction instruction, out MipsExecution<T> execution)
        {
            throw new System.NotImplementedException();
        }
    }
}
