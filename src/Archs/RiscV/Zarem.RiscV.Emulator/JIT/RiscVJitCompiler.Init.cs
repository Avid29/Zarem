// Avishai Dernis 2026

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
        switch (versionInfo.Base)
        {
            case RiscVBaseVersion.RV32:
                InitBitManipulationTable<int>(versionInfo);
                InitMultTable<int, ulong, long>(versionInfo);
                break;
            case RiscVBaseVersion.RV64:
                InitBitManipulationTable<long>(versionInfo);
                InitMultTable<long, UInt128, Int128>(versionInfo);
                break;
            case RiscVBaseVersion.RV128:
                InitBitManipulationTable<Int128>(versionInfo);
                break;
        }
    }

    private void InitBaseTable(RiscVVersionInfo versionInfo)
    {
        // Add ALU Immediate operations in the base register size
        switch (versionInfo.Base)
        {
            case RiscVBaseVersion.RV32:
                InitAluOperations<T, int>(RiscVOpCode.Op, RiscVOpCode.OpImmediate);
                break;
            case RiscVBaseVersion.RV64:
                InitAluOperations<T, long>(RiscVOpCode.Op, RiscVOpCode.OpImmediate);
                break;
            case RiscVBaseVersion.RV128:
                InitAluOperations<T, Int128>(RiscVOpCode.Op, RiscVOpCode.OpImmediate);
                break;
        }

        // Add system operations
        _instructionTable.Register(RiscVOpCode.System, Funct3Code.EcallBreak, (il, inst, pc, compressed) => EmitTrapRet(il, inst.Immediate is 1 ? RiscVTrap.Breakpoint : RiscVTrap.EnvironmentCallFromUMode, pc));

        // Add Jump operations
        _instructionTable.Register(RiscVOpCode.JumpAndLink, JumpAndLink);
        _instructionTable.Register(RiscVOpCode.JumpAndLinkRegister, 0, JumpAndLinkRegister);

        // Add Branch operations
        _instructionTable.Register(RiscVOpCode.Branch, Funct3Code.BranchEqual, (il, inst, pc, compressed) => Branch(il, inst, pc, compressed, OpCodes.Beq));
        _instructionTable.Register(RiscVOpCode.Branch, Funct3Code.BranchNotEqual, (il, inst, pc, compressed) => Branch(il, inst, pc, compressed, OpCodes.Bne_Un));
        _instructionTable.Register(RiscVOpCode.Branch, Funct3Code.BranchLessThan, (il, inst, pc, compressed) => Branch(il, inst, pc, compressed, OpCodes.Blt));
        _instructionTable.Register(RiscVOpCode.Branch, Funct3Code.BranchGreaterThanOrEqual, (il, inst, pc, compressed) => Branch(il, inst, pc, compressed, OpCodes.Bge));
        _instructionTable.Register(RiscVOpCode.Branch, Funct3Code.BranchLessThanUnsigned, (il, inst, pc, compressed) => Branch(il, inst, pc, compressed, OpCodes.Blt_Un));
        _instructionTable.Register(RiscVOpCode.Branch, Funct3Code.BranchGreaterThanOrEqualUnsigned, (il, inst, pc, compressed) => Branch(il, inst, pc, compressed, OpCodes.Bge_Un));

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
        if (!versionInfo.HasExtensions(RiscVExtensions.Multiplication))
            return;

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

    private void InitBitManipulationTable<TSigned>(RiscVVersionInfo versionInfo)
        where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>
    {
        InitBitManipulationOperations<T, TSigned>(versionInfo, RiscVOpCode.Op, RiscVOpCode.OpImmediate);

        if (versionInfo.Base is >= RiscVBaseVersion.RV64)
        {
            // Add explicitly 32-bit operations
            InitBitManipulationOperations<uint, int>(versionInfo, RiscVOpCode.Op32, RiscVOpCode.OpImmediate32);
        }
        if (versionInfo.Base is >= RiscVBaseVersion.RV128)
        {
            // Add explicitly 64-bit operations
            InitBitManipulationOperations<ulong, long>(versionInfo, RiscVOpCode.Op64, RiscVOpCode.OpImmediate64);
        }
    }

    private void InitAluOperations<T2, T2Signed>(RiscVOpCode rOpCode, RiscVOpCode iOpCode)
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
        where T2Signed : unmanaged, IBinaryInteger<T2Signed>, ISignedNumber<T2Signed>
    {
        // R Type
        _instructionTable.Register(rOpCode, Funct3Code.Arithmetic, (context, inst, _, _) => AluR<T2Signed>(context, inst, OpCodes.Add), (context, inst, _, _) => AluR<T2Signed>(context, inst, OpCodes.Sub));
        _instructionTable.Register(rOpCode, Funct3Code.ShiftLeft, (context, inst, _, _) => AluR<T2Signed>(context, inst, OpCodes.Shl));
        _instructionTable.Register(rOpCode, Funct3Code.SetLessThan, (context, inst, _, _) => AluR<T2Signed>(context, inst, OpCodes.Clt));
        _instructionTable.Register(rOpCode, Funct3Code.SetLessThanUnsigned, (context, inst, _, _) => AluR<T2Signed>(context, inst, OpCodes.Clt_Un));
        _instructionTable.Register(rOpCode, Funct3Code.Xor, (context, inst, _, _) => AluR<T2>(context, inst, OpCodes.Xor));
        _instructionTable.Register(rOpCode, Funct3Code.ShiftRight, (context, inst, _, _) => AluR<T2Signed>(context, inst, OpCodes.Shr_Un), (context, inst, _, _) => AluR<T2Signed>(context, inst, OpCodes.Shr));
        _instructionTable.Register(rOpCode, Funct3Code.Or, (context, inst, _, _) => AluR<T2>(context, inst, OpCodes.Or));
        _instructionTable.Register(rOpCode, Funct3Code.And, (context, inst, _, _) => AluR<T2>(context, inst, OpCodes.And));
        
        // I Type
        _instructionTable.Register(iOpCode, Funct3Code.Arithmetic, (context, inst, _, _) => AluI<T2Signed>(context, inst, OpCodes.Add));
        _instructionTable.Register(iOpCode, Funct3Code.ShiftLeft, 0, (context, inst, _, _) => ShiftI<T2Signed>(context, inst, OpCodes.Shl));
        _instructionTable.Register(iOpCode, Funct3Code.SetLessThan, (context, inst, _, _) => AluI<T2Signed>(context, inst, OpCodes.Clt));
        _instructionTable.Register(iOpCode, Funct3Code.SetLessThanUnsigned, (context, inst, _, _) => AluI<T2Signed>(context, inst, OpCodes.Clt_Un));
        _instructionTable.Register(iOpCode, Funct3Code.Xor, (context, inst, _, _) => AluI<T2>(context, inst, OpCodes.Xor));
        _instructionTable.Register(iOpCode, Funct3Code.ShiftRight, 0, (context, inst, _, _) => ShiftI<T2Signed>(context, inst, OpCodes.Shr_Un));
        _instructionTable.Register(iOpCode, Funct3Code.ShiftRight, 32, (context, inst, _, _) => ShiftI<T2Signed>(context, inst, OpCodes.Shr));
        _instructionTable.Register(iOpCode, Funct3Code.Or, (context, inst, _, _) => AluI<T2>(context, inst, OpCodes.Or));
        _instructionTable.Register(iOpCode, Funct3Code.And, (context, inst, _, _) => AluI<T2>(context, inst, OpCodes.And));
    }

    private void InitMultiplyAluOperations<T2, T2Signed, T2Long, T2SignedLong>(RiscVOpCode opCode)
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
        where T2Signed : unmanaged, IBinaryInteger<T2Signed>, ISignedNumber<T2Signed>
        where T2Long : unmanaged, IBinaryInteger<T2Long>, IUnsignedNumber<T2Long>
        where T2SignedLong : unmanaged, IBinaryInteger<T2SignedLong>, ISignedNumber<T2SignedLong>
    {
        _instructionTable.Register(Funct7Code.MExtension, opCode, Funct3Code.Multiply, (context, inst, _, _) => AluR<T2Signed>(context, inst, OpCodes.Mul));
        _instructionTable.Register(Funct7Code.MExtension, opCode, Funct3Code.MultiplyHigh, (context, inst, _, _) => MulH<T2Signed, T2SignedLong>(context, inst));
        _instructionTable.Register(Funct7Code.MExtension, opCode, Funct3Code.MultiplyHighSignedUnsigned, (context, inst, _, _) => MulSH<T2, T2Long, T2SignedLong>(context, inst));
        _instructionTable.Register(Funct7Code.MExtension, opCode, Funct3Code.MultiplyHighUnsigned, (context, inst, _, _) => MulH<T2, T2Long>(context, inst));
        _instructionTable.Register(Funct7Code.MExtension, opCode, Funct3Code.Divide, (context, inst, _, _) => AluR<T2Signed>(context, inst, OpCodes.Div));
        _instructionTable.Register(Funct7Code.MExtension, opCode, Funct3Code.DivideUnsigned, (context, inst, _, _) => AluR<T2Signed>(context, inst, OpCodes.Div_Un));
        _instructionTable.Register(Funct7Code.MExtension, opCode, Funct3Code.Remainder, (context, inst, _, _) => AluR<T2Signed>(context, inst, OpCodes.Rem));
        _instructionTable.Register(Funct7Code.MExtension, opCode, Funct3Code.RemainderUnsigned, (context, inst, _, _) => AluR<T2>(context, inst, OpCodes.Rem_Un));
    }

    private void InitBitManipulationOperations<T2, T2Signed>(RiscVVersionInfo versionInfo, RiscVOpCode rOpCode, RiscVOpCode iOpCode)
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
        where T2Signed : unmanaged, IBinaryInteger<T2Signed>, ISignedNumber<T2Signed>
    {
        // Zba
        if (versionInfo.HasExtensions(RiscVZExtensions.AddressGeneration))
        {
            _instructionTable.Register(Funct7Code.ShiftAdd, rOpCode, Funct3Code.Shift1Add, (context, inst, _, _) => ShiftAdd<T2>(context, inst, 1));
            _instructionTable.Register(Funct7Code.ShiftAdd, rOpCode, Funct3Code.Shift2Add, (context, inst, _, _) => ShiftAdd<T2>(context, inst, 2));
            _instructionTable.Register(Funct7Code.ShiftAdd, rOpCode, Funct3Code.Shift3Add, (context, inst, _, _) => ShiftAdd<T2>(context, inst, 3));
        }

        // Zbb
        if (versionInfo.HasExtensions(RiscVZExtensions.BasicBitManipulation))
        {
            _instructionTable.Register(Funct7Code.BitManipulationCountRotate, rOpCode, Funct3Code.BitCountSignExtendRotateLeft, BitCountSignExtend<T2>);
            _instructionTable.Register(Funct7Code.BitManipulationCountRotate, rOpCode, Funct3Code.RotateRight, (context, inst, _, _) => MethodBinary<T2, int>(context, inst, nameof(T.RotateRight)));
            _instructionTable.Register(Funct7Code.MinMaxClmul, rOpCode, Funct3Code.Min, (context, inst, _, _) => MethodBinary<T2Signed>(context, inst, nameof(T.Min)));
            _instructionTable.Register(Funct7Code.MinMaxClmul, rOpCode, Funct3Code.MinUnsigned, (context, inst, _, _) => MethodBinary<T2>(context, inst, nameof(T.Min)));
            _instructionTable.Register(Funct7Code.MinMaxClmul, rOpCode, Funct3Code.Max, (context, inst, _, _) => MethodBinary<T2Signed>(context, inst, nameof(T.Max)));
            _instructionTable.Register(Funct7Code.MinMaxClmul, rOpCode, Funct3Code.MaxUnsigned, (context, inst, _, _) => MethodBinary<T2>(context, inst, nameof(T.Max)));

            // Overwrite bitwise as modifyable
            _instructionTable.Register(rOpCode, Funct3Code.Xor, (context, inst, _, _) => AluR<T2>(context, inst, OpCodes.Xor), (context, inst, _, _) => AluR<T2>(context, inst, OpCodes.Xor, true));
            _instructionTable.Register(rOpCode, Funct3Code.Or, (context, inst, _, _) => AluR<T2>(context, inst, OpCodes.Or), (context, inst, _, _) => AluR<T2>(context, inst, OpCodes.Or, true));
            _instructionTable.Register(rOpCode, Funct3Code.And, (context, inst, _, _) => AluR<T2>(context, inst, OpCodes.And), (context, inst, _, _) => AluR<T2>(context, inst, OpCodes.And, true));
        }

        // Zbs
        if (versionInfo.HasExtensions(RiscVZExtensions.SingleBitManipulation))
        {
            // R Type
            _instructionTable.Register(Funct7Code.ZbsBClr, rOpCode, Funct3Code.BitClear, (context, inst, _, _) => BitSetClearR<T2>(context, inst, false));
            _instructionTable.Register(Funct7Code.ZbsBSet, rOpCode, Funct3Code.BitSet, (context, inst, _, _) => BitSetClearR<T2>(context, inst, true));
            _instructionTable.Register(Funct7Code.ZbsBInv, rOpCode, Funct3Code.BitInvert, (context, inst, _, _) => BitInvertR<T2>(context, inst));
            _instructionTable.Register(Funct7Code.ZbsBExt, rOpCode, Funct3Code.BitExtract, (context, inst, _, _) => BitExtractR<T2>(context, inst));

            // I Type
            _instructionTable.Register(Funct7Code.ZbsBClr, iOpCode, Funct3Code.BitClear, (context, inst, _, _) => BitSetClearI<T2>(context, inst, false));
            _instructionTable.Register(Funct7Code.ZbsBSet, iOpCode, Funct3Code.BitSet, (context, inst, _, _) => BitSetClearI<T2>(context, inst, true));
            _instructionTable.Register(Funct7Code.ZbsBInv, iOpCode, Funct3Code.BitInvert, (context, inst, _, _) => BitInvertI<T2>(context, inst));
            _instructionTable.Register(Funct7Code.ZbsBExt, iOpCode, Funct3Code.BitExtract, (context, inst, _, _) => BitExtractI<T2>(context, inst));
        }
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
        _instructionTable.Register(format, FloatFunc5Code.Add, (context, inst, _, _) => FloatAlu<TFormat>(context, inst, OpCodes.Add));
        _instructionTable.Register(format, FloatFunc5Code.Subtract, (context, inst, _, _) => FloatAlu<TFormat>(context, inst, OpCodes.Sub));
        _instructionTable.Register(format, FloatFunc5Code.Multiply, (context, inst, _, _) => FloatAlu<TFormat>(context, inst, OpCodes.Mul));
        _instructionTable.Register(format, FloatFunc5Code.Divide, (context, inst, _, _) => FloatAlu<TFormat>(context, inst, OpCodes.Div));
        _instructionTable.Register(format, FloatFunc5Code.MinMax, (context, inst, pc, compressed) => FloatMinMax<TFormat>(context, inst, pc, compressed));
        _instructionTable.Register(format, FloatFunc5Code.SquareRoot, (context, inst, pc, _) => FloatUnary<TFormat>(context, inst, pc, nameof(TFormat.Sqrt)));
        _instructionTable.Register(format, FloatFunc5Code.ConvertToInt, (context, inst, pc, compressed) => FloatConvertFrom<TFormat>(context, inst, pc, compressed));
        _instructionTable.Register(format, FloatFunc5Code.Classify, (context, inst, pc, compressed) => FloatMacGuffin<TFormat>(context, inst, pc, compressed));
        // TODO: MoveFToX
        _instructionTable.Register(format, FloatFunc5Code.Compare, FloatFunct3Code.FloatLessOrEqual, (context, inst, _, _) => FloatFle<TFormat>(context, inst));
        _instructionTable.Register(format, FloatFunc5Code.Compare, FloatFunct3Code.FloatLessThan, (context, inst, _, _) => FloatCompare<TFormat>(context, inst, OpCodes.Clt));
        _instructionTable.Register(format, FloatFunc5Code.Compare, FloatFunct3Code.FloatEqual, (context, inst, _, _) => FloatCompare<TFormat>(context, inst, OpCodes.Ceq));
    }
}
