// Avishai Dernis 2026

using System;
using System.Numerics;
using Zarem.Emulator.Config;
using Zarem.Emulator.Interpret;
using Zarem.Emulator.Machine.Enums;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Functions;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Versioning;
using Zarem.Models.Versioning.Enums;

namespace Zarem.Emulator.Models;

public unsafe partial class RiscVInstructionServiceTable<T, TSigned>
{
    private void InitTables(RiscVEmulatorConfig config)
    {
        var versionInfo = config.VersionInfo;

        // Set default behavior to use empty table
        for (int i = 0; i < 128; i++)
        {
            _func7Table[i] = _emptyTable;
        }

        // Set default behavior to illegal instruction trap, and modified to use the same array as base
        var @base = _func7Table[(int)Funct7Code.Base] = new delegate*<RiscVInstructionServiceTable<T, TSigned>, RiscVInstruction, out RiscVExecution<T>, RiscVTrap>[1024];
        _func7Table[(int)Funct7Code.Modified] = @base;
        for (int i = 0; i < 1024; i++)
        {
            @base[i] = &IllegalInstruction;
            _emptyTable[i] = &IllegalInstruction;
        }

        // Populate base table
        InitBaseTable(versionInfo);

        // Init
        if (versionInfo.Extensions.HasFlag(RiscVExtensions.Multiplication))
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
        var @base = _func7Table[(int)Funct7Code.Base];

        // Add ALU Immediate operations in the base register size
        InitAluOperations<T, TSigned>(RiscVOpCode.Op, RiscVOpCode.OpImmediate);

        // Add system operations
        @base[GetLookupIndex(RiscVOpCode.System, Funct3Code.EcallBreak)] = &EcallBreak;
         
        // Add Jump operations
        @base[GetLookupIndex(RiscVOpCode.JumpAndLink, 0)] = &JumpAndLink;
        @base[GetLookupIndex(RiscVOpCode.JumpAndLinkRegister, 0)] = &JumpAndLinkRegister;
         
        // Add Branch operations
        @base[GetLookupIndex(RiscVOpCode.Branch, Funct3Code.BranchEqual)] = &BranchOn<XeqLogic<T>>;
        @base[GetLookupIndex(RiscVOpCode.Branch, Funct3Code.BranchNotEqual)] = &BranchOn<XneLogic<T>>;
        @base[GetLookupIndex(RiscVOpCode.Branch, Funct3Code.BranchLessThan)] = &BranchOn<XltLogic<T, TSigned>>;
        @base[GetLookupIndex(RiscVOpCode.Branch, Funct3Code.BranchGreaterThanOrEqual)] = &BranchOn<XgeLogic<T, TSigned>>;
        @base[GetLookupIndex(RiscVOpCode.Branch, Funct3Code.BranchLessThanUnsigned)] = &BranchOn<XltuLogic<T>>;
        @base[GetLookupIndex(RiscVOpCode.Branch, Funct3Code.BranchGreaterThanOrEqualUnsigned)] = &BranchOn<XgeuLogic<T>>;

        // Add memory operations
        @base[GetLookupIndex(RiscVOpCode.Load, Funct3Code.LoadByte)] = &Load<sbyte>;
        @base[GetLookupIndex(RiscVOpCode.Load, Funct3Code.LoadHalfWord)] = &Load<short>;
        @base[GetLookupIndex(RiscVOpCode.Load, Funct3Code.LoadWord)] = &Load<int>;
        @base[GetLookupIndex(RiscVOpCode.Load, Funct3Code.LoadByteUnsigned)] = &Load<byte>;
        @base[GetLookupIndex(RiscVOpCode.Load, Funct3Code.LoadHalfWordUnsigned)] = &Load<ushort>;
        @base[GetLookupIndex(RiscVOpCode.Store, Funct3Code.StoreByte)] = &Store<sbyte>;
        @base[GetLookupIndex(RiscVOpCode.Store, Funct3Code.StoreHalfWord)] = &Store<short>;
        @base[GetLookupIndex(RiscVOpCode.Store, Funct3Code.StoreWord)] = &Store<int>;

        // Misc
        @base[GetLookupIndex(RiscVOpCode.LoadUpperImmediate, 0)] = &Lui;

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
        var mulTable = _func7Table[(int)Funct7Code.MExtension] = new delegate*<RiscVInstructionServiceTable<T, TSigned>, RiscVInstruction, out RiscVExecution<T>, RiscVTrap>[1024];
        for (var i = 0; i < 1024; i++)
        {
            mulTable[i] = &IllegalInstruction;
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
        @base[GetLookupIndex(rOpCode, Funct3Code.Arithmetic)] = &ModifyableAluR<AddLogic<T2>, SubLogic<T2>, T2>;
        @base[GetLookupIndex(rOpCode, Funct3Code.ShiftLeft)] = &ShiftR<SllLogic<T2>, T2>;
        @base[GetLookupIndex(rOpCode, Funct3Code.SetLessThan)] = &AluR<SltLogic<T2Signed>, T2Signed>;
        @base[GetLookupIndex(rOpCode, Funct3Code.SetLessThanUnsigned)] = &AluR<SltLogic<T2>, T2>;
        @base[GetLookupIndex(rOpCode, Funct3Code.Xor)] = &AluR<XorLogic<T2>, T2>;
        @base[GetLookupIndex(rOpCode, Funct3Code.ShiftRight)] = &ModifyableShiftR<SrlLogic<T2>, SraLogic<T2Signed>, T2, T2Signed>;
        @base[GetLookupIndex(rOpCode, Funct3Code.Or)] = &AluR<OrLogic<T2>, T2>;
        @base[GetLookupIndex(rOpCode, Funct3Code.And)] = &AluR<AndLogic<T2>, T2>;
        @base[GetLookupIndex(iOpCode, Funct3Code.Arithmetic)] = &AluI<AddLogic<T2>, T2>;
        @base[GetLookupIndex(iOpCode, Funct3Code.ShiftLeft)] = &ShiftI<SllLogic<T2>, T2>;
        @base[GetLookupIndex(iOpCode, Funct3Code.SetLessThan)] = &AluISigned<SltLogic<T2Signed>, T2Signed>;
        @base[GetLookupIndex(iOpCode, Funct3Code.SetLessThanUnsigned)] = &AluI<SltLogic<T2>, T2>;
        @base[GetLookupIndex(iOpCode, Funct3Code.Xor)] = &AluI<XorLogic<T2>, T2>;
        @base[GetLookupIndex(iOpCode, Funct3Code.ShiftRight)] = &ModifyableShiftI<SrlLogic<T2>, SraLogic<T2Signed>, T2, T2Signed>;
        @base[GetLookupIndex(iOpCode, Funct3Code.Or)] = &AluI<OrLogic<T2>, T2>;
        @base[GetLookupIndex(iOpCode, Funct3Code.And)] = &AluI<AndLogic<T2>, T2>;
    }

    private void InitMultiplyAluOperations<T2, T2Signed, T2Long, T2SignedLong>(RiscVOpCode opCode)
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
        where T2Signed : unmanaged, IBinaryInteger<T2Signed>, ISignedNumber<T2Signed>
        where T2Long : struct, IBinaryInteger<T2Long>
        where T2SignedLong : struct, IBinaryInteger<T2SignedLong>
    {
        var mulTable = _func7Table[(int)Funct7Code.MExtension];
        mulTable[GetLookupIndex(opCode, Funct3Code.Multiply)] = &AluR<MulLogic<T2Signed>, T2Signed>;
        mulTable[GetLookupIndex(opCode, Funct3Code.MultiplyHigh)] = &AluR<MulhLogic<T2Signed, T2SignedLong>, T2Signed>;
        mulTable[GetLookupIndex(opCode, Funct3Code.MultiplyHighSignedUnsigned)] = &AluR<MulhsuLogic<T2Signed, T2SignedLong>, T2Signed>;
        mulTable[GetLookupIndex(opCode, Funct3Code.MultiplyHighUnsigned)] = &AluR<MulhLogic<T2, T2Long>, T2>;
        mulTable[GetLookupIndex(opCode, Funct3Code.Divide)] = &AluR<DivLogic<T2Signed>, T2Signed>;
        mulTable[GetLookupIndex(opCode, Funct3Code.DivideUnsigned)] = &AluR<DivLogic<T2>, T2>;
        mulTable[GetLookupIndex(opCode, Funct3Code.Remainder)] = &AluR<RemLogic<T2Signed>, T2Signed>;
        mulTable[GetLookupIndex(opCode, Funct3Code.RemainderUnsigned)] = &AluR<RemLogic<T2>, T2>;
    }
}
