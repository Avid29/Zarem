// Avishai Dernis 2026

using Zarem.Emulator.Config;
using Zarem.Models.Instructions.Enums.Operations;

namespace Zarem.Emulator.Models;

public unsafe partial class RiscVInstructionServiceTable<T, TSigned>
{
    private void InitTables(RiscVEmulatorConfig config)
    {
        var version = config.VersionInfo;

        // Set default behavior to reserve instruction trap
        for (int i = 0; i < 64; i++)
        {
            _opCodeTable[i] = &Illegal;
        }

        for (int i = 0; i < 8; i++)
        {
            _aluImmTable[i] = &Illegal;
            _aluRegTable[i] = &Illegal;
            _loadTable[i] = &Illegal;
            _storeTable[i] = &Illegal;
            _branchTable[i] = &Illegal;
        }

        // Populate tables
        InitOpCode();
    }

    private void InitOpCode()
    {
        _opCodeTable[(int)OperationCode.Load] = &DipatchLoad;
        _opCodeTable[(int)OperationCode.Store] = &DipatchStore;
        _opCodeTable[(int)OperationCode.AluImmediate] = &DipatchAluImmediate;
        _opCodeTable[(int)OperationCode.AluImmediate] = &DipatchAluRegister;
    }
}
