// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Numerics;
using Zarem.Emulator.Machine.Registers;
using Zarem.RiscV.Emulator.Interpret;
using Zarem.RiscV.Emulator.Machine.Enums;
using Zarem.RiscV.Models.Instructions;

namespace Zarem.Emulator.Models;

public partial class RiscVInstructionServiceTable<T, TSigned>
{
    private static RiscVTrap FloatAlu<TLogic, TFormat>(RiscVInterpretCpu<T> cpu, RiscVFloatInstruction inst, out RiscVExecution<T> exec)
        where TLogic : struct, IAluLogic<TFormat>
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        var indexer = GetFloatRegisterIndexer<TFormat>(cpu);
        var frs1 = indexer[(int)inst.FRS1];
        var frs2 = indexer[(int)inst.FRS2];
        var value = TLogic.Compute(frs1, frs2);
        exec = RiscVExecution<T>.CreateFloatWriteback<TFormat>(inst.FRD, value);
        return RiscVTrap.None;
    }

    private static RiscVTrap FloatFAlu<TLogic, TFormat>(RiscVInterpretCpu<T> cpu, RiscVFloatInstruction inst, out RiscVExecution<T> exec)
        where TLogic : struct, IFAluLogic<TFormat>
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        var indexer = GetFloatRegisterIndexer<TFormat>(cpu);
        var frs1 = indexer[(int)inst.FRS1];
        var value = TLogic.Compute(frs1);
        exec = RiscVExecution<T>.CreateFloatWriteback<TFormat>(inst.FRD, value);
        return RiscVTrap.None;
    }

    private static IFormattedRegisterIndexer<TFormat> GetFloatRegisterIndexer<TFormat>(RiscVInterpretCpu<T> cpu)
        where TFormat : unmanaged, INumber<TFormat>
    {
#if DEBUG
        Guard.IsNotNull(cpu.FloatRegisterFile);
#endif

        if (typeof(TFormat) == typeof(float)) return (IFormattedRegisterIndexer<TFormat>)cpu.FloatRegisterFile.Singles;
        else if (typeof(TFormat) == typeof(double)) return (IFormattedRegisterIndexer<TFormat>)cpu.FloatRegisterFile.Doubles;
        else if (typeof(TFormat) == typeof(int)) return (IFormattedRegisterIndexer<TFormat>)cpu.FloatRegisterFile.Words;
        else if (typeof(TFormat) == typeof(long)) return (IFormattedRegisterIndexer<TFormat>)cpu.FloatRegisterFile.Longs;
        else throw new InvalidOperationException();
    }
}
