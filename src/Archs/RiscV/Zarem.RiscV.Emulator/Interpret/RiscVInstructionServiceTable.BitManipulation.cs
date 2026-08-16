// Avishai Dernis 2026

using System.Numerics;
using Zarem.RiscV.Emulator.Interpret;
using Zarem.RiscV.Emulator.Machine.Enums;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums.Functions;

namespace Zarem.Emulator.Models;

public partial class RiscVInstructionServiceTable<T, TFloat, TSigned>
{
    private static RiscVTrap BitCountSignExtend<T2, T2Signed>(RiscVInterpretCpu<T, TFloat> cpu, RiscVInstruction inst, bool compressed, out RiscVExecution<T> exec)
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
        where T2Signed : unmanaged, IBinaryInteger<T2Signed>, ISignedNumber<T2Signed>
    {
        return inst.RSCode switch
        {
            FunctRS2Code.CountLeadingZeros => AluR<ClzLogic<T2>, T2>(cpu, inst, compressed, out exec),
            FunctRS2Code.CountTrailingZeros => AluR<CtzLogic<T2>, T2>(cpu, inst, compressed, out exec),
            FunctRS2Code.PopulationCount => AluR<CpopLogic<T2>, T2>(cpu, inst, compressed, out exec),
            FunctRS2Code.SignExtendByte => AluR<Sext<T2, T2Signed, sbyte>, T2>(cpu, inst, compressed, out exec),
            FunctRS2Code.SignExtendHalfword => AluR<Sext<T2, T2Signed, short>, T2>(cpu, inst, compressed, out exec),
            _ => IllegalInstruction(cpu, inst, compressed, out exec),
        };
    }
}
