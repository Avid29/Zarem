// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Models.Versioning;
using Zarem.RiscV.Emulator.Config;
using Zarem.RiscV.Emulator.Interpret;
using Zarem.RiscV.Emulator.Machine.Enums;
using Zarem.RiscV.Models;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums;
using Zarem.RiscV.Models.Instructions.Enums.Functions;
using Zarem.RiscV.Models.Instructions.Enums.Operations;
using Zarem.RiscV.Models.Versioning.Enums;

namespace Zarem.Emulator.Models;

public unsafe partial class RiscVInstructionServiceTable<T, TFloat, TSigned>
{
    private void InitTables(RiscVEmulatorConfig config)
    {
        var versionInfo = config.VersionInfo;

        InitBaseTable(versionInfo);
        InitFloatTable(versionInfo);

        if (versionInfo.Extensions.Flags.HasFlag(RiscVExtensions.Multiplication))
        {
            switch (versionInfo.Base)
            {
                case RiscVBaseVersion.RV32:
                    InitMultTable<ulong, long>(versionInfo);
                    break;
                case RiscVBaseVersion.RV64:
                    InitMultTable<UInt128, Int128>(versionInfo);
                    break;
                case RiscVBaseVersion.RV128:
                    InitMultTable<BigInteger, BigInteger>(versionInfo);
                    break;
            }
        }
    }

    private void InitBaseTable(RiscVVersionInfo versionInfo)
    {
        // Add ALU Immediate operations in the base register size
        InitAluOperations<T, TSigned>(RiscVOpCode.Op, RiscVOpCode.OpImmediate);

        // Add system operations
        Register(RiscVOpCode.System, Funct3Code.EcallBreak, &EcallBreak);

        // Add Jump operations
        Register(RiscVOpCode.JumpAndLink, &JumpAndLink);
        Register(RiscVOpCode.JumpAndLinkRegister, 0, &JumpAndLinkRegister);
         
        // Add Branch operations
        Register(RiscVOpCode.Branch, Funct3Code.BranchEqual, &BranchOn<XeqLogic<T>>);
        Register(RiscVOpCode.Branch, Funct3Code.BranchNotEqual, &BranchOn<XneLogic<T>>);
        Register(RiscVOpCode.Branch, Funct3Code.BranchLessThan, &BranchOn<XltLogic<T, TSigned>>);
        Register(RiscVOpCode.Branch, Funct3Code.BranchGreaterThanOrEqual, &BranchOn<XgeLogic<T, TSigned>>);
        Register(RiscVOpCode.Branch, Funct3Code.BranchLessThanUnsigned, &BranchOn<XltuLogic<T>>);
        Register(RiscVOpCode.Branch, Funct3Code.BranchGreaterThanOrEqualUnsigned, &BranchOn<XgeuLogic<T>>);

        // Add memory operations
        Register(RiscVOpCode.Load, Funct3Code.LoadByte, &Load<sbyte>);
        Register(RiscVOpCode.Load, Funct3Code.LoadHalfWord, &Load<short>);
        Register(RiscVOpCode.Load, Funct3Code.LoadWord, &Load<int>);
        Register(RiscVOpCode.Load, Funct3Code.LoadByteUnsigned, &Load<byte>);
        Register(RiscVOpCode.Load, Funct3Code.LoadHalfWordUnsigned, &Load<ushort>);
        Register(RiscVOpCode.Store, Funct3Code.StoreByte, &Store<sbyte>);
        Register(RiscVOpCode.Store, Funct3Code.StoreHalfWord, &Store<short>);
        Register(RiscVOpCode.Store, Funct3Code.StoreWord, &Store<int>);

        // Misc
        Register(RiscVOpCode.LoadUpperImmediate, 0, &Lui);

        if (versionInfo.Base is >= RiscVBaseVersion.RV64)
        {
            // Add explicitly 32-bit ALU operations
            InitAluOperations<uint, int>(RiscVOpCode.Op32, RiscVOpCode.OpImmediate32);
        }
        if (versionInfo.Base is >= RiscVBaseVersion.RV128)
        {
            // Add explicitly 64-bit ALU operations
            InitAluOperations<ulong, long>(RiscVOpCode.Op64, RiscVOpCode.OpImmediate64);
        }
    }

    private void InitMultTable<TLong, TSignedLong>(RiscVVersionInfo versionInfo)
        where TLong : struct, IBinaryInteger<TLong>
        where TSignedLong : struct, IBinaryInteger<TSignedLong>
    {
        InitMultiplyAluOperations<T, TSigned, TLong, TSignedLong>(RiscVOpCode.Op);

        if (versionInfo.Base is >= RiscVBaseVersion.RV64)
        {
            // Add explicitly 32-bit operations
            InitMultiplyAluOperations<uint, int, ulong, long>(RiscVOpCode.Op32);
        }
        if (versionInfo.Base is >= RiscVBaseVersion.RV128)
        {
            // Add explicitly 64-bit operations
            InitMultiplyAluOperations<ulong, long, UInt128, Int128>(RiscVOpCode.Op64);
        }
    }

    private void InitAluOperations<T2, T2Signed>(RiscVOpCode rOpCode, RiscVOpCode iOpCode)
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
        where T2Signed : unmanaged, IBinaryInteger<T2Signed>, ISignedNumber<T2Signed>
    {
        Register(rOpCode, Funct3Code.Arithmetic, &ModifyableAluR<AddLogic<T2>, SubLogic<T2>, T2>);
        Register(rOpCode, Funct3Code.ShiftLeft, &ShiftR<SllLogic<T2>, T2>);
        Register(rOpCode, Funct3Code.SetLessThan, &AluR<SltLogic<T2Signed>, T2Signed>);
        Register(rOpCode, Funct3Code.SetLessThanUnsigned, &AluR<SltLogic<T2>, T2>);
        Register(rOpCode, Funct3Code.Xor, &AluR<XorLogic<T2>, T2>);
        Register(rOpCode, Funct3Code.ShiftRight, &ModifyableShiftR<SrlLogic<T2>, SraLogic<T2Signed>, T2, T2Signed>);
        Register(rOpCode, Funct3Code.Or, &AluR<OrLogic<T2>, T2>);
        Register(rOpCode, Funct3Code.And, &AluR<AndLogic<T2>, T2>);
        Register(iOpCode, Funct3Code.Arithmetic, &AluI<AddLogic<T2>, T2>);
        Register(iOpCode, Funct3Code.ShiftLeft, &ShiftI<SllLogic<T2>, T2>);
        Register(iOpCode, Funct3Code.SetLessThan, &AluISigned<SltLogic<T2Signed>, T2Signed>);
        Register(iOpCode, Funct3Code.SetLessThanUnsigned, &AluI<SltLogic<T2>, T2>);
        Register(iOpCode, Funct3Code.Xor, &AluI<XorLogic<T2>, T2>);
        Register(iOpCode, Funct3Code.ShiftRight, &ModifyableShiftI<SrlLogic<T2>, SraLogic<T2Signed>, T2, T2Signed>);
        Register(iOpCode, Funct3Code.Or, &AluI<OrLogic<T2>, T2>);
        Register(iOpCode, Funct3Code.And, &AluI<AndLogic<T2>, T2>);
    }

    private void InitMultiplyAluOperations<T2, T2Signed, T2Long, T2SignedLong>(RiscVOpCode opCode)
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
        where T2Signed : unmanaged, IBinaryInteger<T2Signed>, ISignedNumber<T2Signed>
        where T2Long : struct, IBinaryInteger<T2Long>
        where T2SignedLong : struct, IBinaryInteger<T2SignedLong>
    {
        Register(Funct7Code.MExtension, opCode, Funct3Code.Multiply, &AluR<MulLogic<T2Signed>, T2Signed>);
        Register(Funct7Code.MExtension, opCode, Funct3Code.MultiplyHigh, &AluR<MulhLogic<T2Signed, T2SignedLong>, T2Signed>);
        Register(Funct7Code.MExtension, opCode, Funct3Code.MultiplyHighSignedUnsigned, &AluR<MulhsuLogic<T2Signed, T2SignedLong>, T2Signed>);
        Register(Funct7Code.MExtension, opCode, Funct3Code.MultiplyHighUnsigned, &AluR<MulhLogic<T2, T2Long>, T2>);
        Register(Funct7Code.MExtension, opCode, Funct3Code.Divide, &AluR<DivLogic<T2Signed>, T2Signed>);
        Register(Funct7Code.MExtension, opCode, Funct3Code.DivideUnsigned, &AluR<DivLogic<T2>, T2>);
        Register(Funct7Code.MExtension, opCode, Funct3Code.Remainder, &AluR<RemLogic<T2Signed>, T2Signed>);
        Register(Funct7Code.MExtension, opCode, Funct3Code.RemainderUnsigned, &AluR<RemLogic<T2>, T2>);
    }

    private void InitFloatTable(RiscVVersionInfo versionInfo)
    {
        InitFloatOperations<Half>(versionInfo, RiscVExtensions.HalfPrecisionFloatingPoint);
        InitFloatOperations<float>(versionInfo, RiscVExtensions.SingleFloatingPoint);
        InitFloatOperations<double>(versionInfo, RiscVExtensions.DoubleFloatingPoint);
    }

    private void InitFloatOperations<TFormat>(RiscVVersionInfo versionInfo, RiscVExtensions flag)
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        if (!versionInfo.Extensions.Flags.HasFlag(flag))
            return;

        var format = RiscVInstructionDecodeTable<T>.GetFloatFuncTableIndex<TFormat>();
        Register(format, FloatFunc5Code.Add, &FloatAlu<AddLogic<TFormat>, TFormat>);
        Register(format, FloatFunc5Code.Subtract, &FloatAlu<SubLogic<TFormat>, TFormat>);
        Register(format, FloatFunc5Code.Multiply, &FloatAlu<MulLogic<TFormat>, TFormat>);
        Register(format, FloatFunc5Code.Divide, &FloatAlu<DivLogic<TFormat>, TFormat>);
        Register(format, FloatFunc5Code.MinMax, &FloatMinMax<TFormat>);
        Register(format, FloatFunc5Code.SquareRoot, &FloatFAlu<SqrtLogic<TFormat>, TFormat>);
        Register(format, FloatFunc5Code.ConvertToInt, &FloatConvertFrom<TFormat>);
        Register(format, FloatFunc5Code.Classify, &FloatMacGuffin<TFormat>);
        Register(format, FloatFunc5Code.MoveFToX, &FloatMoveFrom<TFormat>);
        Register(format, FloatFunc5Code.Compare, FloatFunct3Code.FloatLessOrEqual, &FloatCompare<TFormat>);
        Register(format, FloatFunc5Code.Compare, FloatFunct3Code.FloatLessThan, &FloatCompare<TFormat>);
        Register(format, FloatFunc5Code.Compare, FloatFunct3Code.FloatEqual, &FloatCompare<TFormat>);
    }

    private void Register(RiscVOpCode opCode, delegate*<RiscVInterpretCpu<T, TFloat>, RiscVInstruction, out RiscVExecution<T>, RiscVTrap> func)
        => _instructionTable.Register(opCode, (IntPtr)func);

    private void Register(RiscVOpCode opCode, Funct3Code funct3, delegate*<RiscVInterpretCpu<T, TFloat>, RiscVInstruction, out RiscVExecution<T>, RiscVTrap> func)
        => _instructionTable.Register(opCode, funct3, (IntPtr)func);

    private void Register(Funct7Code funct7, RiscVOpCode opCode, Funct3Code funct3, delegate*<RiscVInterpretCpu<T, TFloat>, RiscVInstruction, out RiscVExecution<T>, RiscVTrap> func)
        => _instructionTable.Register(funct7, opCode, funct3, (IntPtr)func);

    private void Register(RiscVFloatFormat format, FloatFunc5Code funct5, FloatFunct3Code funct3, delegate*<RiscVInterpretCpu<T, TFloat>, RiscVFloatInstruction, out RiscVExecution<T>, RiscVTrap> func)
        => _instructionTable.Register(format, funct5, funct3, (IntPtr)func);

    private void Register(RiscVFloatFormat format, FloatFunc5Code funct5, delegate*<RiscVInterpretCpu<T, TFloat>, RiscVFloatInstruction, out RiscVExecution<T>, RiscVTrap> func)
        => _instructionTable.Register(format, funct5, (IntPtr)func);

    private static IntPtr GetFunctionPtrValue(delegate*<RiscVInterpretCpu<T, TFloat>, RiscVInstruction, out RiscVExecution<T>, RiscVTrap> func)
        => (IntPtr)func;
}
