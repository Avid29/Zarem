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
        InitAluImmediateOperations<T, TS>(OperationCode.AluImmediate);
        InitAluRegisterOperations<T, TS>(OperationCode.Alu);

        // Add branch operations
        _baseTable[GetLookupIndex(OperationCode.Branch, Funct3Code.BranchEqual)] = &BranchOn<XeqLogic<T>>;
        _baseTable[GetLookupIndex(OperationCode.Branch, Funct3Code.BranchNotEqual)] = &BranchOn<XneLogic<T>>;
        _baseTable[GetLookupIndex(OperationCode.Branch, Funct3Code.BranchLessThan)] = &BranchOn<XltLogic<T, TS>>;
        _baseTable[GetLookupIndex(OperationCode.Branch, Funct3Code.BranchGreaterThanOrEqual)] = &BranchOn<XgeLogic<T, TS>>;
        _baseTable[GetLookupIndex(OperationCode.Branch, Funct3Code.BranchLessThanUnsigned)] = &BranchOn<XltuLogic<T>>;
        _baseTable[GetLookupIndex(OperationCode.Branch, Funct3Code.BranchGreaterThanOrEqualUnsigned)] = &BranchOn<XgeuLogic<T>>;

        _baseTable[GetLookupIndex(OperationCode.JumpAndLink, 0)] = 

        if (baseVersion is >= RiscVBaseVersion.RV64)
        {
            // Add explicitly 32-bit ALU operations
            InitAluImmediateOperations<uint, int>(OperationCode.AluImmediate32);
            InitAluRegisterOperations<uint, int>(OperationCode.Alu32);
        }

        if (baseVersion is >= RiscVBaseVersion.RV128)
        {
            // Add explicitly 64-bit ALU operations
            InitAluImmediateOperations<ulong, long>(OperationCode.AluImmediate64);
            InitAluRegisterOperations<ulong, long>(OperationCode.Alu64);
        }
    }

    private void InitAluImmediateOperations<T2, TS2>(OperationCode opCode)
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
        where TS2 : unmanaged, IBinaryInteger<TS2>, ISignedNumber<TS2>
    {
        // Base
        _baseTable[GetLookupIndex(opCode, Funct3Code.Arithmetic)] = &AluI<AdduLogic<T2>, T2>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.ShiftLeft)] = &ShiftI<SllLogic<T2>, T2>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.SetLessThan)] = &AluISigned<SltLogic<T2, TS2>, T2, TS2>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.SetLessThanUnsigned)] = &AluI<SltuLogic<T2>, T2>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.Xor)] = &AluI<XorLogic<T2>, T2>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.ShiftRight)] = &ModifyableShiftI<SrlLogic<T2>, SraLogic<T2, TS2>, T2>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.Or)] = &AluI<OrLogic<T2>, T2>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.And)] = &AluI<AndLogic<T2>, T2>;
    }

    private void InitAluRegisterOperations<T2, TS2>(OperationCode opCode)
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
        where TS2 : unmanaged, IBinaryInteger<TS2>, ISignedNumber<TS2>
    {
        // Base
        _baseTable[GetLookupIndex(opCode, Funct3Code.Arithmetic)] = &ModifyableAluR<AdduLogic<T2>, SubuLogic<T2>, T2>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.ShiftLeft)] = &ShiftR<SllLogic<T2>, T2>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.SetLessThan)] = &AluR<SltLogic<T2, TS2>, T2>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.SetLessThanUnsigned)] = &AluR<SltuLogic<T2>, T2>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.Xor)] = &AluR<XorLogic<T2>, T2>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.ShiftRight)] = &ModifyableShiftR<SrlLogic<T2>, SraLogic<T2, TS2>, T2>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.Or)] = &AluR<OrLogic<T2>, T2>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.And)] = &AluR<AndLogic<T2>, T2>;
    }
}
