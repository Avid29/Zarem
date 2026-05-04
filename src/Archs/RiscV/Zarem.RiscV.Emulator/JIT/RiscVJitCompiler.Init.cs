// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Numerics;
using System.Reflection.Emit;
using Zarem.Emulator.Config;
using Zarem.Emulator.Machine.Enums;
using Zarem.Models.Instructions.Enums.Functions;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Versioning;
using Zarem.Models.Versioning.Enums;

namespace Zarem.Emulator.JIT;

public partial class RiscVJitCompiler<T>
{
    private void InitTables(RiscVEmulatorConfig config)
    {
        var versionInfo = config.VersionInfo;

        // Set default behavior to use empty table
        for (int i = 0; i < 128; i++)
        {
            _func7Table[i] = _emptyTable;
        }

        // Set default behavior to illegal instruction trap
        var @base = _func7Table[(int)Funct7Code.Base] = new RiscVEmitter[1024];
        _func7Table[(int)Funct7Code.Modified] = @base;
        for (int i = 0; i < 1024; i++)
        {
            @base[i] = IllegalInstruction;
            _emptyTable[i] = IllegalInstruction;
        }

        // Populate base table
        InitBaseTable(versionInfo);

        // Init
        if (versionInfo.Extensions.HasFlag(RiscVExtensions.Multiplication))
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
        var @base = _func7Table[(int)Funct7Code.Base];

        // Add ALU Immediate operations in the base register size
        switch (versionInfo.Base)
        {
            case RiscVBaseVersion.RV32: InitAluOperations<T, int>(RiscVOpCode.Op, RiscVOpCode.OpImmediate); break;
            case RiscVBaseVersion.RV64: InitAluOperations<T, long>(RiscVOpCode.Op, RiscVOpCode.OpImmediate); break;
            case RiscVBaseVersion.RV128: InitAluOperations<T, Int128>(RiscVOpCode.Op, RiscVOpCode.OpImmediate); break;
        }

        // Add system operations
        @base[GetLookupIndex(RiscVOpCode.System, Funct3Code.EcallBreak)] = (il, inst, pc) => EmitTrapRet(il, inst.Immediate is 1 ? RiscVTrap.Breakpoint : RiscVTrap.EnvironmentCallFromUMode, pc);

        // Add Jump operations
        @base[GetLookupIndex(RiscVOpCode.JumpAndLink, 0)] = JumpAndLink;

        // Add Branch operations
        @base[GetLookupIndex(RiscVOpCode.Branch, Funct3Code.BranchEqual)] = (il, inst, pc) => Branch(il, inst, pc, OpCodes.Beq);
        @base[GetLookupIndex(RiscVOpCode.Branch, Funct3Code.BranchNotEqual)] = (il, inst, pc) => Branch(il, inst, pc, OpCodes.Bne_Un);
        @base[GetLookupIndex(RiscVOpCode.Branch, Funct3Code.BranchLessThan)] = (il, inst, pc) => Branch(il, inst, pc, OpCodes.Blt);
        @base[GetLookupIndex(RiscVOpCode.Branch, Funct3Code.BranchGreaterThanOrEqual)] = (il, inst, pc) => Branch(il, inst, pc, OpCodes.Bge);
        @base[GetLookupIndex(RiscVOpCode.Branch, Funct3Code.BranchLessThanUnsigned)] = (il, inst, pc) => Branch(il, inst, pc, OpCodes.Blt_Un);
        @base[GetLookupIndex(RiscVOpCode.Branch, Funct3Code.BranchGreaterThanOrEqualUnsigned)] = (il, inst, pc) => Branch(il, inst, pc, OpCodes.Bge_Un);

        // Add memory operations
        @base[GetLookupIndex(RiscVOpCode.Load, Funct3Code.LoadByte)] = Load<sbyte>;
        @base[GetLookupIndex(RiscVOpCode.Load, Funct3Code.LoadHalfWord)] = Load<short>;
        @base[GetLookupIndex(RiscVOpCode.Load, Funct3Code.LoadWord)] = Load<int>;
        @base[GetLookupIndex(RiscVOpCode.Load, Funct3Code.LoadByteUnsigned)] = Load<byte>;
        @base[GetLookupIndex(RiscVOpCode.Load, Funct3Code.LoadHalfWordUnsigned)] = Load<ushort>;
        @base[GetLookupIndex(RiscVOpCode.Store, Funct3Code.StoreByte)] = Store<sbyte>;
        @base[GetLookupIndex(RiscVOpCode.Store, Funct3Code.StoreHalfWord)] = Store<short>;
        @base[GetLookupIndex(RiscVOpCode.Store, Funct3Code.StoreWord)] = Store<int>;

        // Add misc operations
        @base[GetLookupIndex(RiscVOpCode.LoadUpperImmediate, 0)] = Lui;

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
        var mulTable = _func7Table[(int)Funct7Code.MExtension] = new RiscVEmitter[1024];
        for (var i = 0; i < 1024; i++)
        {
            mulTable[i] = IllegalInstruction;
        }

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
        var @base = _func7Table[(int)Funct7Code.Base];
        @base[GetLookupIndex(rOpCode, Funct3Code.Arithmetic)] = (il, inst, pc) => AluR<T2Signed>(il, inst, inst.Funct7 is Funct7Code.Modified ? OpCodes.Sub : OpCodes.Add);
        @base[GetLookupIndex(rOpCode, Funct3Code.ShiftLeft)] = (il, inst, pc) => AluR<T2Signed>(il, inst, OpCodes.Shl);
        @base[GetLookupIndex(rOpCode, Funct3Code.SetLessThan)] = (il, inst, pc) => AluR<T2Signed>(il, inst, OpCodes.Clt);
        @base[GetLookupIndex(rOpCode, Funct3Code.SetLessThanUnsigned)] = (il, inst, pc) => AluR<T2Signed>(il, inst, OpCodes.Clt_Un);
        @base[GetLookupIndex(rOpCode, Funct3Code.Xor)] = (il, inst, pc) => AluR<T2Signed>(il, inst, OpCodes.Xor);
        @base[GetLookupIndex(rOpCode, Funct3Code.ShiftRight)] = (il, inst, pc) => AluR<T2Signed>(il, inst, inst.Funct7 is Funct7Code.Modified ? OpCodes.Shr : OpCodes.Shr_Un);
        @base[GetLookupIndex(rOpCode, Funct3Code.Or)] = (il, inst, pc) => AluR<T2Signed>(il, inst, OpCodes.Or);
        @base[GetLookupIndex(rOpCode, Funct3Code.And)] = (il, inst, pc) => AluR<T2Signed>(il, inst, OpCodes.And);
        @base[GetLookupIndex(iOpCode, Funct3Code.Arithmetic)] = (il, inst, pc) => AluI<T2Signed>(il, inst, OpCodes.Add);
        @base[GetLookupIndex(iOpCode, Funct3Code.ShiftLeft)] = (il, inst, pc) => ShiftI<T2Signed>(il, inst, OpCodes.Shl);
        @base[GetLookupIndex(iOpCode, Funct3Code.SetLessThan)] = (il, inst, pc) => AluI<T2Signed>(il, inst, OpCodes.Clt);
        @base[GetLookupIndex(iOpCode, Funct3Code.SetLessThanUnsigned)] = (il, inst, pc) => AluI<T2Signed>(il, inst, OpCodes.Clt_Un);
        @base[GetLookupIndex(iOpCode, Funct3Code.Xor)] = (il, inst, pc) => AluI<T2Signed>(il, inst, OpCodes.Xor);
        @base[GetLookupIndex(iOpCode, Funct3Code.ShiftRight)] = (il, inst, pc) => ShiftI<T2Signed>(il, inst, inst.Funct7 is Funct7Code.Modified ? OpCodes.Shr : OpCodes.Shr_Un);
        @base[GetLookupIndex(iOpCode, Funct3Code.Or)] = (il, inst, pc) => AluI<T2Signed>(il, inst, OpCodes.Or);
        @base[GetLookupIndex(iOpCode, Funct3Code.And)] = (il, inst, pc) => AluI<T2Signed>(il, inst, OpCodes.And);
    }

    private void InitMultiplyAluOperations<T2, T2Signed, T2Long, T2SignedLong>(RiscVOpCode opCode)
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
        where T2Signed : unmanaged, IBinaryInteger<T2Signed>, ISignedNumber<T2Signed>
        where T2Long : unmanaged, IBinaryInteger<T2Long>, IUnsignedNumber<T2Long>
        where T2SignedLong : unmanaged, IBinaryInteger<T2SignedLong>, ISignedNumber<T2SignedLong>
    {
        var mulTable = _func7Table[(int)Funct7Code.MExtension];
        mulTable[GetLookupIndex(opCode, Funct3Code.Multiply)] = (il, inst, pc) => AluR<T2Signed>(il, inst, OpCodes.Mul);
        mulTable[GetLookupIndex(opCode, Funct3Code.MultiplyHigh)] = (il, inst, pc) => MulH<T2Signed, T2SignedLong>(il, inst);
        mulTable[GetLookupIndex(opCode, Funct3Code.MultiplyHighSignedUnsigned)] = (il, inst, pc) => MulSH<T2, T2Long, T2SignedLong>(il, inst);
        mulTable[GetLookupIndex(opCode, Funct3Code.MultiplyHighUnsigned)] = (il, inst, pc) => MulH<T2, T2Long>(il, inst);
        mulTable[GetLookupIndex(opCode, Funct3Code.Divide)] = (il, inst, pc) => AluR<T2Signed>(il, inst, OpCodes.Div);
        mulTable[GetLookupIndex(opCode, Funct3Code.DivideUnsigned)] = (il, inst, pc) => AluR<T2Signed>(il, inst, OpCodes.Div_Un);
        mulTable[GetLookupIndex(opCode, Funct3Code.Remainder)] = (il, inst, pc) => AluR<T2Signed>(il, inst, OpCodes.Rem);
        mulTable[GetLookupIndex(opCode, Funct3Code.RemainderUnsigned)] = (il, inst, pc) => AluR<T2>(il, inst, OpCodes.Rem_Un);
    }
}
