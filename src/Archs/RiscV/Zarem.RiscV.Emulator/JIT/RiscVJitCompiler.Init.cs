// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Numerics;
using System.Reflection.Emit;
using Zarem.Models.Versioning;
using Zarem.RiscV.Emulator.Config;
using Zarem.RiscV.Emulator.Machine.Enums;
using Zarem.RiscV.Models;
using Zarem.RiscV.Models.Instructions.Enums.Functions;
using Zarem.RiscV.Models.Instructions.Enums.Operations;
using Zarem.RiscV.Models.Versioning.Enums;

namespace Zarem.RiscV.Emulator.JIT;

public partial class RiscVJitCompiler<T, TFloat>
{
    private void InitTables(RiscVEmulatorConfig config)
    {
        var versionInfo = config.VersionInfo;

        // Populate base table
        InitBaseTable(versionInfo);
        InitFloatTable(versionInfo);

        // Init
        if (versionInfo.HasExtensions(RiscVExtensions.Multiplication))
        {
            switch (versionInfo.Base)
            {
                case RiscVBaseVersion.RV32:
                    InitMultTable<int, ulong, long>(versionInfo);
                    break;
                case RiscVBaseVersion.RV64:
                    InitMultTable<long, UInt128, Int128>(versionInfo);
                    break;
                case RiscVBaseVersion.RV128:
                    ThrowHelper.ThrowArgumentException();
                    break;
            }
        }
    }

    private void InitBaseTable(RiscVVersionInfo versionInfo)
    {
        // Add ALU Immediate operations in the base register size
        switch (versionInfo.Base)
        {
            case RiscVBaseVersion.RV32: InitAluOperations<T, int>(RiscVOpCode.Op, RiscVOpCode.OpImmediate); break;
            case RiscVBaseVersion.RV64: InitAluOperations<T, long>(RiscVOpCode.Op, RiscVOpCode.OpImmediate); break;
            case RiscVBaseVersion.RV128: InitAluOperations<T, Int128>(RiscVOpCode.Op, RiscVOpCode.OpImmediate); break;
        }

        // Add system operations
        _instructionTable.Register(RiscVOpCode.System, Funct3Code.EcallBreak, (il, inst, pc) => EmitTrapRet(il, inst.Immediate is 1 ? RiscVTrap.Breakpoint : RiscVTrap.EnvironmentCallFromUMode, pc));

        // Add Jump operations
        _instructionTable.Register(RiscVOpCode.JumpAndLink, JumpAndLink);
        _instructionTable.Register(RiscVOpCode.JumpAndLinkRegister, 0, JumpAndLinkRegister);

        // Add Branch operations
        _instructionTable.Register(RiscVOpCode.Branch, Funct3Code.BranchEqual, (il, inst, pc) => Branch(il, inst, pc, OpCodes.Beq));
        _instructionTable.Register(RiscVOpCode.Branch, Funct3Code.BranchNotEqual, (il, inst, pc) => Branch(il, inst, pc, OpCodes.Bne_Un));
        _instructionTable.Register(RiscVOpCode.Branch, Funct3Code.BranchLessThan, (il, inst, pc) => Branch(il, inst, pc, OpCodes.Blt));
        _instructionTable.Register(RiscVOpCode.Branch, Funct3Code.BranchGreaterThanOrEqual, (il, inst, pc) => Branch(il, inst, pc, OpCodes.Bge));
        _instructionTable.Register(RiscVOpCode.Branch, Funct3Code.BranchLessThanUnsigned, (il, inst, pc) => Branch(il, inst, pc, OpCodes.Blt_Un));
        _instructionTable.Register(RiscVOpCode.Branch, Funct3Code.BranchGreaterThanOrEqualUnsigned, (il, inst, pc) => Branch(il, inst, pc, OpCodes.Bge_Un));

        // Add memory operations
        _instructionTable.Register(RiscVOpCode.Load, Funct3Code.LoadByte, Load<sbyte>);
        _instructionTable.Register(RiscVOpCode.Load, Funct3Code.LoadHalfWord, Load<short>);
        _instructionTable.Register(RiscVOpCode.Load, Funct3Code.LoadWord, Load<int>);
        _instructionTable.Register(RiscVOpCode.Load, Funct3Code.LoadByteUnsigned, Load<byte>);
        _instructionTable.Register(RiscVOpCode.Load, Funct3Code.LoadHalfWordUnsigned, Load<ushort>);
        _instructionTable.Register(RiscVOpCode.Store, Funct3Code.StoreByte, Store<sbyte>);
        _instructionTable.Register(RiscVOpCode.Store, Funct3Code.StoreHalfWord, Store<short>);
        _instructionTable.Register(RiscVOpCode.Store, Funct3Code.StoreWord, Store<int>);

        // Add misc operations
        _instructionTable.Register(RiscVOpCode.LoadUpperImmediate, 0, Lui);

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
     
    private void InitMultTable<TSigned, TLong, TSignedLong>(RiscVVersionInfo versionInfo)
        where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>
        where TLong : unmanaged, IBinaryInteger<TLong>, IUnsignedNumber<TLong>
        where TSignedLong : unmanaged, IBinaryInteger<TSignedLong>, ISignedNumber<TSignedLong>
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
        _instructionTable.Register(rOpCode, Funct3Code.Arithmetic, (il, inst, pc) => AluR<T2Signed>(il, inst, inst.Funct7 is Funct7Code.Modified ? OpCodes.Sub : OpCodes.Add));
        _instructionTable.Register(rOpCode, Funct3Code.ShiftLeft, (il, inst, pc) => AluR<T2Signed>(il, inst, OpCodes.Shl));
        _instructionTable.Register(rOpCode, Funct3Code.SetLessThan, (il, inst, pc) => AluR<T2Signed>(il, inst, OpCodes.Clt));
        _instructionTable.Register(rOpCode, Funct3Code.SetLessThanUnsigned, (il, inst, pc) => AluR<T2Signed>(il, inst, OpCodes.Clt_Un));
        _instructionTable.Register(rOpCode, Funct3Code.Xor, (il, inst, pc) => AluR<T2Signed>(il, inst, OpCodes.Xor));
        _instructionTable.Register(rOpCode, Funct3Code.ShiftRight, (il, inst, pc) => AluR<T2Signed>(il, inst, inst.Funct7 is Funct7Code.Modified ? OpCodes.Shr : OpCodes.Shr_Un));
        _instructionTable.Register(rOpCode, Funct3Code.Or, (il, inst, pc) => AluR<T2Signed>(il, inst, OpCodes.Or));
        _instructionTable.Register(rOpCode, Funct3Code.And, (il, inst, pc) => AluR<T2Signed>(il, inst, OpCodes.And));
        _instructionTable.Register(iOpCode, Funct3Code.Arithmetic, (il, inst, pc) => AluI<T2Signed>(il, inst, OpCodes.Add));
        _instructionTable.Register(iOpCode, Funct3Code.ShiftLeft, (il, inst, pc) => ShiftI<T2Signed>(il, inst, OpCodes.Shl));
        _instructionTable.Register(iOpCode, Funct3Code.SetLessThan, (il, inst, pc) => AluI<T2Signed>(il, inst, OpCodes.Clt));
        _instructionTable.Register(iOpCode, Funct3Code.SetLessThanUnsigned, (il, inst, pc) => AluI<T2Signed>(il, inst, OpCodes.Clt_Un));
        _instructionTable.Register(iOpCode, Funct3Code.Xor, (il, inst, pc) => AluI<T2Signed>(il, inst, OpCodes.Xor));
        _instructionTable.Register(iOpCode, Funct3Code.ShiftRight, (il, inst, pc) => ShiftI<T2Signed>(il, inst, inst.Funct7 is Funct7Code.Modified ? OpCodes.Shr : OpCodes.Shr_Un));
        _instructionTable.Register(iOpCode, Funct3Code.Or, (il, inst, pc) => AluI<T2Signed>(il, inst, OpCodes.Or));
        _instructionTable.Register(iOpCode, Funct3Code.And, (il, inst, pc) => AluI<T2Signed>(il, inst, OpCodes.And));
    }

    private void InitMultiplyAluOperations<T2, T2Signed, T2Long, T2SignedLong>(RiscVOpCode opCode)
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
        where T2Signed : unmanaged, IBinaryInteger<T2Signed>, ISignedNumber<T2Signed>
        where T2Long : unmanaged, IBinaryInteger<T2Long>, IUnsignedNumber<T2Long>
        where T2SignedLong : unmanaged, IBinaryInteger<T2SignedLong>, ISignedNumber<T2SignedLong>
    {
        _instructionTable.Register(Funct7Code.MExtension, opCode, Funct3Code.Multiply, (il, inst, pc) => AluR<T2Signed>(il, inst, OpCodes.Mul));
        _instructionTable.Register(Funct7Code.MExtension, opCode, Funct3Code.MultiplyHigh, (il, inst, pc) => MulH<T2Signed, T2SignedLong>(il, inst));
        _instructionTable.Register(Funct7Code.MExtension, opCode, Funct3Code.MultiplyHighSignedUnsigned, (il, inst, pc) => MulSH<T2, T2Long, T2SignedLong>(il, inst));
        _instructionTable.Register(Funct7Code.MExtension, opCode, Funct3Code.MultiplyHighUnsigned, (il, inst, pc) => MulH<T2, T2Long>(il, inst));
        _instructionTable.Register(Funct7Code.MExtension, opCode, Funct3Code.Divide, (il, inst, pc) => AluR<T2Signed>(il, inst, OpCodes.Div));
        _instructionTable.Register(Funct7Code.MExtension, opCode, Funct3Code.DivideUnsigned, (il, inst, pc) => AluR<T2Signed>(il, inst, OpCodes.Div_Un));
        _instructionTable.Register(Funct7Code.MExtension, opCode, Funct3Code.Remainder, (il, inst, pc) => AluR<T2Signed>(il, inst, OpCodes.Rem));
        _instructionTable.Register(Funct7Code.MExtension, opCode, Funct3Code.RemainderUnsigned, (il, inst, pc) => AluR<T2>(il, inst, OpCodes.Rem_Un));
    }

    private void InitFloatTable(RiscVVersionInfo versionInfo)
    {
        InitFloatOperations<Half>(versionInfo, RiscVZExtensions.HalfPrecisionFloatingPoint);
        InitFloatOperations<float>(versionInfo, RiscVExtensions.SingleFloatingPoint);
        InitFloatOperations<double>(versionInfo, RiscVExtensions.DoubleFloatingPoint);
    }

    private void InitFloatOperations<TFormat>(RiscVVersionInfo versionInfo, RiscVExtensionInfo extension)
        where TFormat : unmanaged, IBinaryFloatingPointIeee754<TFormat>
    {
        if (!versionInfo.HasExtensions(extension))
            return;

        var format = RiscVInstructionDecodeTable<T>.GetFloatFuncTableIndex<TFormat>();
        _instructionTable.Register(format, FloatFunc5Code.Add, (il, inst, pc) => FloatAlu<TFormat>(il, inst, OpCodes.Add));
        _instructionTable.Register(format, FloatFunc5Code.Subtract, (il, inst, pc) => FloatAlu<TFormat>(il, inst, OpCodes.Sub));
        _instructionTable.Register(format, FloatFunc5Code.Multiply, (il, inst, pc) => FloatAlu<TFormat>(il, inst, OpCodes.Mul));
        _instructionTable.Register(format, FloatFunc5Code.Divide, (il, inst, pc) => FloatAlu<TFormat>(il, inst, OpCodes.Div));
        _instructionTable.Register(format, FloatFunc5Code.MinMax, (il, inst, pc) => FloatMinMax<TFormat>(il, inst, pc));
        _instructionTable.Register(format, FloatFunc5Code.SquareRoot, (il, inst, pc) => FloatUnary<TFormat>(il, inst, pc, nameof(TFormat.Sqrt)));
        _instructionTable.Register(format, FloatFunc5Code.ConvertToInt, (il, inst, pc) => FloatConvertFrom<TFormat>(il, inst, pc));
        _instructionTable.Register(format, FloatFunc5Code.Classify, (il, inst, pc) => FloatMacGuffin<TFormat>(il, inst, pc));
        // TODO: MoveFToX
        _instructionTable.Register(format, FloatFunc5Code.Compare, FloatFunct3Code.FloatLessOrEqual, (il, inst, pc) => FloatFle<TFormat>(il, inst));
        _instructionTable.Register(format, FloatFunc5Code.Compare, FloatFunct3Code.FloatLessThan, (il, inst, pc) => FloatCompare<TFormat>(il, inst, OpCodes.Clt));
        _instructionTable.Register(format, FloatFunc5Code.Compare, FloatFunct3Code.FloatEqual, (il, inst, pc) => FloatCompare<TFormat>(il, inst, OpCodes.Ceq));
    }
}
