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
            _modifiedTable[i] = &IllegalInstruction;
        }

        // Populate tables
        InitBaseTable(version.Base);
    }

    /// <remarks>
    /// This will populate both <see cref="_baseTable"/> and <see cref="_modifiedTable"/>.
    /// </remarks>
    private void InitBaseTable(RiscVBaseVersion baseVersion)
    {
        // Hook up tables
        _funct7Table[(int)Funct7Code.Base] = &DispatchBaseTable;
        _funct7Table[(int)Funct7Code.Modified] = &DispatchModifiedTable;

        // Add ALU operations in the base register size
        InitAluImmediateOperations<T, TS>(OperationCode.AluImmediate);
        InitAluRegisterOperations<T, TS>(OperationCode.Alu);

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
        _baseTable[GetLookupIndex(opCode, Funct3Code.ShiftLeft)] = &Shift<SllLogic<T2>, T2>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.SetLessThan)] = &AluISigned<SltLogic<T2, TS2>, T2, TS2>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.SetLessThanUnsigned)] = &AluI<SltuLogic<T2>, T2>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.Xor)] = &AluI<XorLogic<T2>, T2>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.ShiftRight)] = &Shift<SrlLogic<T2>, T2>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.Or)] = &AluI<XorLogic<T2>, T2>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.And)] = &AluI<XorLogic<T2>, T2>;

        // Modified
        _modifiedTable[GetLookupIndex(opCode, Funct3Code.ShiftRight)] = &Shift<SraLogic<T2, TS2>, T2>;
    }

    private void InitAluRegisterOperations<T2, TS2>(OperationCode opCode)
        where T2 : unmanaged, IBinaryInteger<T2>, IUnsignedNumber<T2>
        where TS2 : unmanaged, IBinaryInteger<TS2>, ISignedNumber<TS2>
    {
        // Base
        _baseTable[GetLookupIndex(opCode, Funct3Code.Arithmetic)] = &AluR<AdduLogic<T2>, T2>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.ShiftLeft)] = &ShiftVar<SllLogic<T2>, T2>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.SetLessThan)] = &AluR<SltLogic<T2, TS2>, T2>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.SetLessThanUnsigned)] = &AluR<SltuLogic<T2>, T2>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.Xor)] = &AluR<XorLogic<T2>, T2>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.ShiftRight)] = &ShiftVar<SrlLogic<T2>, T2>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.Or)] = &AluR<XorLogic<T2>, T2>;
        _baseTable[GetLookupIndex(opCode, Funct3Code.And)] = &AluR<XorLogic<T2>, T2>;

        // Modified
        _modifiedTable[GetLookupIndex(opCode, Funct3Code.Arithmetic)] = &AluR<SubuLogic<T2>, T2>;
        _modifiedTable[GetLookupIndex(opCode, Funct3Code.ShiftRight)] = &ShiftVar<SraLogic<T2, TS2>, T2>;
    }
}
