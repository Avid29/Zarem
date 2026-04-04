// Avishai Dernis 2026

using Zarem.Emulator.Config;
using Zarem.Models.Instructions.Enums.Functions;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Versioning.Enums;

namespace Zarem.Emulator.Models;

public unsafe partial class RiscVInstructionServiceTable<T, TSigned>
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

        // Alu Immediate
        _baseTable[GetLookupIndex(OperationCode.AluImmediate, Funct3Code.Arithmetic)] = &AluI<AdduLogic<T>, T>;
        _baseTable[GetLookupIndex(OperationCode.AluImmediate, Funct3Code.ShiftLeft)] = &Shift<SllLogic<T>, T>;
        _baseTable[GetLookupIndex(OperationCode.AluImmediate, Funct3Code.SetLessThan)] = &AluISigned<SltLogic<T, TSigned>, T, TSigned>;
        _baseTable[GetLookupIndex(OperationCode.AluImmediate, Funct3Code.SetLessThanUnsigned)] = &AluI<SltuLogic<T>, T>;
        _baseTable[GetLookupIndex(OperationCode.AluImmediate, Funct3Code.Xor)] = &AluI<XorLogic<T>, T>;
        _baseTable[GetLookupIndex(OperationCode.AluImmediate, Funct3Code.ShiftRight)] = &Shift<SrlLogic<T>, T>;
        _modifiedTable[GetLookupIndex(OperationCode.AluImmediate, Funct3Code.Arithmetic)] = &AluI<SubuLogic<T>, T>;
        _modifiedTable[GetLookupIndex(OperationCode.AluImmediate, Funct3Code.ShiftRight)] = &Shift<SraLogic<T, TSigned>, T>;

        // Alu
        _baseTable[GetLookupIndex(OperationCode.Alu, Funct3Code.Arithmetic)] = &AluR<AdduLogic<T>, T>;
        _baseTable[GetLookupIndex(OperationCode.Alu, Funct3Code.ShiftLeft)] = &ShiftVar<SllLogic<T>, T>;
        _baseTable[GetLookupIndex(OperationCode.Alu, Funct3Code.SetLessThan)] = &AluR<SltLogic<T, TSigned>, T>;
        _baseTable[GetLookupIndex(OperationCode.Alu, Funct3Code.SetLessThanUnsigned)] = &AluR<SltuLogic<T>, T>;
        _baseTable[GetLookupIndex(OperationCode.Alu, Funct3Code.Xor)] = &AluR<XorLogic<T>, T>;
        _baseTable[GetLookupIndex(OperationCode.Alu, Funct3Code.ShiftRight)] = &ShiftVar<SrlLogic<T>, T>;
        _modifiedTable[GetLookupIndex(OperationCode.Alu, Funct3Code.Arithmetic)] = &AluR<SubuLogic<T>, T>;
        _modifiedTable[GetLookupIndex(OperationCode.Alu, Funct3Code.ShiftRight)] = &ShiftVar<SraLogic<T, TSigned>, T>;

        if (baseVersion is >= RiscVBaseVersion.RV64)
        {
            // Alu Immediate 32
            _baseTable[GetLookupIndex(OperationCode.AluImmediate32, Funct3Code.Arithmetic)] = &AluI<AdduLogic<uint>, uint>;
            _baseTable[GetLookupIndex(OperationCode.AluImmediate32, Funct3Code.ShiftLeft)] = &Shift<SllLogic<uint>, uint>;
            _modifiedTable[GetLookupIndex(OperationCode.AluImmediate, Funct3Code.Arithmetic)] = &AluI<SubuLogic<uint>, uint>;

            // Alu 32
            _baseTable[GetLookupIndex(OperationCode.Alu32, Funct3Code.Arithmetic)] = &AluR<AdduLogic<uint>, uint>;
            _baseTable[GetLookupIndex(OperationCode.Alu32, Funct3Code.ShiftLeft)] = &ShiftVar<SllLogic<uint>, uint>;
            _modifiedTable[GetLookupIndex(OperationCode.AluImmediate, Funct3Code.Arithmetic)] = &AluR<SubuLogic<uint>, uint>;
        }

        if (baseVersion is >= RiscVBaseVersion.RV128)
        {
            // Alu Immediate 64
            _baseTable[GetLookupIndex(OperationCode.AluImmediate64, Funct3Code.Arithmetic)] = &AluI<AdduLogic<ulong>, ulong>;
            _baseTable[GetLookupIndex(OperationCode.AluImmediate64, Funct3Code.ShiftLeft)] = &Shift<SllLogic<ulong>, ulong>;
            _modifiedTable[GetLookupIndex(OperationCode.AluImmediate, Funct3Code.Arithmetic)] = &AluI<SubuLogic<ulong>, ulong>;

            // Alu 64
            _baseTable[GetLookupIndex(OperationCode.Alu64, Funct3Code.Arithmetic)] = &AluR<AdduLogic<ulong>, ulong>;
            _baseTable[GetLookupIndex(OperationCode.Alu64, Funct3Code.ShiftLeft)] = &ShiftVar<SllLogic<ulong>, ulong>;
            _modifiedTable[GetLookupIndex(OperationCode.AluImmediate, Funct3Code.Arithmetic)] = &AluR<SubuLogic<ulong>, ulong>;
        }
    }
}
