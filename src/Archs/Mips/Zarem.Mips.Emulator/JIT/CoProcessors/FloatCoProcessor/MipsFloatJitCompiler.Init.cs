// Avishai Dernis 2026

using System;
using System.Numerics;
using System.Reflection.Emit;
using Zarem.Mips.Models;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums;
using Zarem.Mips.Models.Instructions.Enums.Functions.FloatProc;
using Zarem.Mips.Models.Versioning;
using Zarem.Mips.Models.Versioning.Enums;

namespace Zarem.Mips.Emulator.JIT.CoProcessors.FloatCoProcessor;

public unsafe partial class MipsFloatJitCompiler<T, TFloat>
{
    private void InitFloat(MipsVersionInfo versionInfo)
    {
        InitFloatRoot(versionInfo);
        InitFloatFuncs<float>(versionInfo);
        InitFloatFuncs<double>(versionInfo);

        InitConvertFuncs<float>(versionInfo);
        InitConvertFuncs<double>(versionInfo);
        InitConvertFuncs<int>(versionInfo);

        if (versionInfo.Is64Bit)
        {
            InitConvertFuncs<long>(versionInfo);
        }
    }

    private void InitFloatRoot(MipsVersionInfo versionInfo)
    {
        _instructionTable.Register(CoProc1RSCode.MFC1, (il, inst, pc) => MoveFromFloat(il, inst));
        _instructionTable.Register(CoProc1RSCode.MTC1, (il, inst, pc) => MoveToFloat(il, inst));
    }

    private void InitFloatFuncs<TFormat>(MipsVersionInfo versionInfo)
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        var format = MipsFloatInstructionDecodeTable<T>.GetFloatFuncTableIndex<TFormat>();
        _instructionTable.Register(format, MipsFloatFuncCode.Add, (il, inst, pc) => FloatAlu<TFormat>(il, inst, OpCodes.Add));
        _instructionTable.Register(format, MipsFloatFuncCode.Subtract, (il, inst, pc) => FloatAlu<TFormat>(il, inst, OpCodes.Sub));
        _instructionTable.Register(format, MipsFloatFuncCode.Multiply, (il, inst, pc) => FloatAlu<TFormat>(il, inst, OpCodes.Mul));
        _instructionTable.Register(format, MipsFloatFuncCode.Divide, (il, inst, pc) => FloatAlu<TFormat>(il, inst, OpCodes.Div));
        _instructionTable.Register(format, MipsFloatFuncCode.SquareRoot, (il, inst, pc) => FloatUnary<TFormat>(il, inst, nameof(Math.Sqrt)));
        _instructionTable.Register(format, MipsFloatFuncCode.AbsoluteValue, (il, inst, pc) => FloatUnary<TFormat>(il, inst, nameof(Math.Abs)));
        _instructionTable.Register(format, MipsFloatFuncCode.Move, (il, inst, pc) => MoveFloat<TFormat>(il, inst.FS, inst.FD));
        _instructionTable.Register(format, MipsFloatFuncCode.Negate, (il, inst, pc) => FloatUnary<TFormat>(il, inst, OpCodes.Neg));

        _instructionTable.Register(format, MipsFloatFuncCode.Round_W, (il, inst, pc) => FloatRound<TFormat, int>(il, inst, nameof(Math.Round)));
        _instructionTable.Register(format, MipsFloatFuncCode.Truncate_W, (il, inst, pc) => FloatRound<TFormat, int>(il, inst, nameof(Math.Truncate)));
        _instructionTable.Register(format, MipsFloatFuncCode.Ceiling_W, (il, inst, pc) => FloatRound<TFormat, int>(il, inst, nameof(Math.Ceiling)));
        _instructionTable.Register(format, MipsFloatFuncCode.Floor_W, (il, inst, pc) => FloatRound<TFormat, int>(il, inst, nameof(Math.Floor)));

        if (versionInfo.Is64Bit)
        {
            _instructionTable.Register(format, MipsFloatFuncCode.Round_L, (il, inst, pc) => FloatRound<TFormat, long>(il, inst, nameof(Math.Round)));
            _instructionTable.Register(format, MipsFloatFuncCode.Truncate_L, (il, inst, pc) => FloatRound<TFormat, long>(il, inst, nameof(Math.Truncate)));
            _instructionTable.Register(format, MipsFloatFuncCode.Ceiling_L, (il, inst, pc) => FloatRound<TFormat, long>(il, inst, nameof(Math.Ceiling)));
            _instructionTable.Register(format, MipsFloatFuncCode.Floor_L, (il, inst, pc) => FloatRound<TFormat, long>(il, inst, nameof(Math.Floor)));
        }

        if (versionInfo.Generation >= MipsGeneration.MipsIV)
        {
            _instructionTable.Register(format, MipsFloatFuncCode.Reciprical, (il, inst, pc) => FloatUnary<TFormat>(il, inst, nameof(Math.ReciprocalEstimate)));
        }

        if (versionInfo.Generation >= MipsGeneration.R2)
        {
            _instructionTable.Register(format, MipsFloatFuncCode.RecipricalSquareRoot, (il, inst, pc) => FloatUnary<TFormat>(il, inst, nameof(Math.ReciprocalSqrtEstimate)));
        }
    }

    private void InitConvertFuncs<TFormat>(MipsVersionInfo versionInfo)
        where TFormat : unmanaged, INumber<TFormat>
    {
        var format = MipsFloatInstructionDecodeTable<T>.GetFloatFuncTableIndex<TFormat>();
        InitConvertFunc<TFormat, float>(format, MipsFloatFuncCode.ConvertToSingle);
        InitConvertFunc<TFormat, double>(format, MipsFloatFuncCode.ConvertToDouble);
        InitConvertFunc<TFormat, int>(format, MipsFloatFuncCode.ConvertToWord);

        if (versionInfo.Is64Bit && typeof(TFormat) != typeof(long))
        {
            InitConvertFunc<TFormat, long>(format, MipsFloatFuncCode.ConvertToLong);
        }
    }

    private void InitConvertFunc<TFrom, TTo>(MipsFloatFormat format, MipsFloatFuncCode code)
        where TFrom : unmanaged, INumber<TFrom>
        where TTo : unmanaged, INumber<TTo>
    {
        if (typeof(TFrom) == typeof(TTo))
            return;

        _instructionTable.Register(format, code, (il, inst, pc) => FloatConvert<TFrom, TTo>(il, ((MipsFloatInstruction)inst).FS, ((MipsFloatInstruction)inst).FD));
    }
}
