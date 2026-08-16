// Avishai Dernis 2026

using Zarem.RiscV.Emulator.Interpret;
using Zarem.RiscV.Emulator.Machine.Enums;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums.Functions;

namespace Zarem.Emulator.Models;

public partial class RiscVInstructionServiceTable<T, TFloat, TSigned>
{
    private static RiscVTrap BitCountSignExtend(RiscVInterpretCpu<T, TFloat> cpu, RiscVInstruction inst, bool compressed, out RiscVExecution<T> exec)
    {
        return inst.RSCode switch
        {
            FunctRS2Code.CountLeadingZeros => AluR<ClzLogic<T>, T>(cpu, inst, compressed, out exec),
            FunctRS2Code.CountTrailingZeros => AluR<CtzLogic<T>, T>(cpu, inst, compressed, out exec),
            FunctRS2Code.PopulationCount => AluR<CpopLogic<T>, T>(cpu, inst, compressed, out exec),
            _ => IllegalInstruction(cpu, inst, compressed, out exec),
        };
    }
}
