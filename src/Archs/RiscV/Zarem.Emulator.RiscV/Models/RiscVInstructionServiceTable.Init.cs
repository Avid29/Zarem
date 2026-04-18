// Avishai Dernis 2026

using System.Numerics;
using Zarem.Emulator.Config;
using Zarem.Models.Instructions.Enums.Functions;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Versioning.Enums;

namespace Zarem.Emulator.Models;

public unsafe partial class RiscVInstructionServiceTable<T, TS>
{
    private void InitTables(RiscVEmulatorConfig config)
    {
        var version = config.VersionInfo;

        // Set default behavior to reserve instruction trap
        for (int i = 0; i < 128; i++)
        {
            _funct7Table[i] = &IllegalInstruction;
        }

        // Set default behavior to illegal instruction trap
        for (int i = 0; i < 1024; i++)
        {
            _baseTable[i] = &IllegalInstruction;
        }

        // Populate tables
        InitBaseTable(version.Base);
    }

    private void InitBaseTable(RiscVBaseVersion baseVersion)
    {
        // Hook up tables
        _funct7Table[(int)Funct7Code.Base] = &DispatchBaseTable;
        _funct7Table[(int)Funct7Code.Modified] = &DispatchBaseTable;

        // Add ALU operations in the base register size
        InitAluImmediateOperations<T, TS>(RiscVOpCode.AluImmediate);
        InitAluRegisterOperations<T, TS>(RiscVOpCode.Alu);

        // Add Jump/Branch operations
        _baseTable[GetLookupIndex(RiscVOpCode.JumpAndLink, 0)] = &JumpAndLink;
        _baseTable[GetLookupIndex(RiscVOpCode.Branch, Funct3Code.BranchEqual)] = &BranchOn<XeqLogic<T>>;
        _baseTable[GetLookupIndex(RiscVOpCode.Branch, Funct3Code.BranchNotEqual)] = &BranchOn<XneLogic<T>>;
        _baseTable[GetLookupIndex(RiscVOpCode.Branch, Funct3Code.BranchLessThan)] = &BranchOn<XltLogic<T, TS>>;
        _baseTable[GetLookupIndex(RiscVOpCode.Branch, Funct3Code.BranchGreaterThanOrEqual)] = &BranchOn<XgeLogic<T, TS>>;
        _baseTable[GetLookupIndex(RiscVOpCode.Branch, Funct3Code.BranchLessThanUnsigned)] = &BranchOn<XltuLogic<T>>;
        _baseTable[GetLookupIndex(RiscVOpCode.Branch, Funct3Code.BranchGreaterThanOrEqualUnsigned)] = &BranchOn<XgeuLogic<T>>;

        // Add system operations
        _baseTable[GetLookupIndex(RiscVOpCode.System, Funct3Code.EcallBreak)] = &EcallBreak;

        if (baseVersion is >= RiscVBaseVersion.RV64)
        {
            // Add explicitly 32-bit ALU operations
            InitAluImmediateOperations<uint, int>(RiscVOpCode.AluImmediate32);
            InitAluRegisterOperations<uint, int>(RiscVOpCode.Alu32);
        }

        if (baseVersion is >= RiscVBaseVersion.RV128)
        {
            // Add explicitly 64-bit ALU operations
            InitAluImmediateOperations<ulong, long>(RiscVOpCode.AluImmediate64);
            InitAluRegisterOperations<ulong, long>(RiscVOpCode.Alu64);
        }
    }

    private void InitAluImmediateOperations<TUnsigned, TSigned>(RiscVOpCode opCode)
        where TUnsigned : unmanaged, IBinaryInteger<TUnsigned>, IUnsignedNumber<TUnsigned>
        where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>
    {
        // Base
        _baseTable[GetLookupIndex(opCode, Funct3Code.Arithmetic)] = &AluI<AddLogic<TUnsigned>, TUnsigned>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.ShiftLeft)] = &ShiftI<SllLogic<TUnsigned>, TUnsigned>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.SetLessThan)] = &AluISigned<SltLogic<TSigned>, TSigned>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.SetLessThanUnsigned)] = &AluI<SltLogic<TUnsigned>, TUnsigned>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.Xor)] = &AluI<XorLogic<TUnsigned>, TUnsigned>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.ShiftRight)] = &ModifyableShiftI<SrlLogic<TUnsigned>, SraLogic<TSigned>, TUnsigned, TSigned>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.Or)] = &AluI<OrLogic<TUnsigned>, TUnsigned>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.And)] = &AluI<AndLogic<TUnsigned>, TUnsigned>;
    }

    private void InitAluRegisterOperations<TUnsigned, TSigned>(RiscVOpCode opCode)
        where TUnsigned : unmanaged, IBinaryInteger<TUnsigned>, IUnsignedNumber<TUnsigned>
        where TSigned : unmanaged, IBinaryInteger<TSigned>, ISignedNumber<TSigned>
    {
        // Base
        _baseTable[GetLookupIndex(opCode, Funct3Code.Arithmetic)] = &ModifyableAluR<AddLogic<TUnsigned>, SubLogic<TUnsigned>, TUnsigned>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.ShiftLeft)] = &ShiftR<SllLogic<TUnsigned>, TUnsigned>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.SetLessThan)] = &AluR<SltLogic<TSigned>, TSigned>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.SetLessThanUnsigned)] = &AluR<SltLogic<TUnsigned>, TUnsigned>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.Xor)] = &AluR<XorLogic<TUnsigned>, TUnsigned>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.ShiftRight)] = &ModifyableShiftR<SrlLogic<TUnsigned>, SraLogic<TSigned>, TUnsigned, TSigned>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.Or)] = &AluR<OrLogic<TUnsigned>, TUnsigned>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.And)] = &AluR<AndLogic<TUnsigned>, TUnsigned>;
    }
}
