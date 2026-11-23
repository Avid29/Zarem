// Avishai Dernis 2026

using System.Numerics;
using Zarem.Mips.Emulator.Machine.CoProcessors;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models.Instructions;

namespace Zarem.Mips.Emulator.Interpret.CoProcessors.FloatCoProcessor;

/// <summary>
/// A <see cref="FloatCoProcessor{T, TFloat}{T}"/> that executes by interpreting each instruction.
/// </summary>
public sealed class InterpretedFloatCoProcessor<T, TFloat> : FloatCoProcessor<T, TFloat>, IInterpretedFloatCoProcessor<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TFloat : unmanaged, IBinaryInteger<TFloat>, IUnsignedNumber<TFloat>
{
    private readonly MipsFloatInstructionServiceTable<T, TFloat> _instructionServiceTable;

    /// <summary>
    /// Initializes a new instance of the <see cref="InterpretedFloatCoProcessor{T, TFloat}"/> class.
    /// </summary>
    public InterpretedFloatCoProcessor(MipsInterpretedCpu<T> cpu) : base(cpu.ControlCoProcessor)
    {
        _instructionServiceTable = new MipsFloatInstructionServiceTable<T, TFloat>(cpu, this);
    }

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
                Words[(int)fExecution.Register] = (int)fExecution.Writeback;
                break;
            case FloatWriteback.Long:
                Longs[(int)fExecution.Register] = fExecution.Writeback;
                break;
            case FloatWriteback.Control:
                ControlRegisterFile[(int)fExecution.Register] = (uint)fExecution.Writeback;
                break;
        }

        return MipsTrap.None;
    }
}
