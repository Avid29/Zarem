// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Emulator.Models.Logic;
using Zarem.Mips.Emulator.Interpret.CoProcessors.FloatCoProcessor;
using Zarem.Mips.Emulator.Machine;
using Zarem.Mips.Emulator.Machine.CoProcessors;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums;
using Zarem.Mips.Models.Instructions.Enums.Functions.FloatProc;
using Zarem.Mips.Models.Versioning;
using Zarem.Mips.Models.Versioning.Enums;

namespace Zarem.Mips.Emulator.Interpret.CoProcessors;

public unsafe partial class MipsFloatInstructionServiceTable<T, TFloat>
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
        Register(CoProc1RSCode.MFC1, &MFC1);
        Register(CoProc1RSCode.CFC1, &CFC1);
        Register(CoProc1RSCode.MTC1, &MTC1);
        Register(CoProc1RSCode.CTC1, &CTC1);
    }

    private void InitFloatFuncs<TFormat>(MipsVersionInfo versionInfo)
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        var format = MipsFloatInstructionDecodeTable<T>.GetFloatFuncTableIndex<TFormat>();
        Register(format, MipsFloatFuncCode.Add, &FloatAlu<AddLogic<TFormat>, TFormat>);
        Register(format, MipsFloatFuncCode.Subtract, &FloatAlu<SubLogic<TFormat>, TFormat>);
        Register(format, MipsFloatFuncCode.Multiply, &FloatAlu<MulLogic<TFormat>, TFormat>);
        Register(format, MipsFloatFuncCode.Divide, &FloatAlu<DivLogic<TFormat>, TFormat>);
        Register(format, MipsFloatFuncCode.SquareRoot, &FloatFAlu<SqrtLogic<TFormat>, TFormat>);
        Register(format, MipsFloatFuncCode.AbsoluteValue, &FloatFAlu<AbsLogic<TFormat>, TFormat>);
        Register(format, MipsFloatFuncCode.Move, &FloatFAlu<MovLogic<TFormat>, TFormat>);
        Register(format, MipsFloatFuncCode.Negate, &FloatFAlu<NegLogic<TFormat>, TFormat>);

        Register(format, MipsFloatFuncCode.Round_W, &FloatRound<RoundLogic<TFormat>, TFormat, int>);
        Register(format, MipsFloatFuncCode.Truncate_W, &FloatRound<TruncLogic<TFormat>, TFormat, int>);
        Register(format, MipsFloatFuncCode.Ceiling_W, &FloatRound<CeilingLogic<TFormat>, TFormat, int>);
        Register(format, MipsFloatFuncCode.Floor_W, &FloatRound<FloorLogic<TFormat>, TFormat, int>);

        if (versionInfo.Is64Bit)
        {
            Register(format, MipsFloatFuncCode.Round_L, &FloatRound<RoundLogic<TFormat>, TFormat, long>);
            Register(format, MipsFloatFuncCode.Truncate_L, &FloatRound<TruncLogic<TFormat>, TFormat, long>);
            Register(format, MipsFloatFuncCode.Ceiling_L, &FloatRound<CeilingLogic<TFormat>, TFormat, long>);
            Register(format, MipsFloatFuncCode.Floor_L, &FloatRound<FloorLogic<TFormat>, TFormat, long>);
        }

        if (versionInfo.Generation >= MipsGeneration.MipsIV)
        {
            Register(format, MipsFloatFuncCode.Reciprical, &FloatFAlu<RecipLogic<TFormat>, TFormat>);
        }

        if (versionInfo.Generation >= MipsGeneration.R2)
        {
            Register(format, MipsFloatFuncCode.RecipricalSquareRoot, &FloatFAlu<RSqrtLogic<TFormat>, TFormat>);
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

        Register(format, code, &FloatConvert<TFrom, TTo>);
    }

    private void Register(CoProc1RSCode funcCode, delegate*<MipsCpu<T>, FloatCoProcessor<T, TFloat>, MipsFloatInstruction, out MipsFloatExecution<T>, MipsTrap> func)
        => _instructionTable.Register(funcCode, (IntPtr)func);

    private void Register(MipsFloatFormat format, MipsFloatFuncCode funcCode, delegate*<MipsCpu<T>, FloatCoProcessor<T, TFloat>, MipsFloatInstruction, out MipsFloatExecution<T>, MipsTrap> func)
        => _instructionTable.Register(format, funcCode, (IntPtr)func);


    private static IntPtr GetFunctionPtrValue(delegate*<MipsCpu<T>, FloatCoProcessor<T, TFloat>, MipsInstruction, out MipsExecution<T>, MipsTrap> func)
        => (IntPtr)func;
}
