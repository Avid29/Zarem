// Avishai Dernis 2026

using System.Runtime.CompilerServices;
using Zarem.Emulator.Machine.Registers;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Mips.Emulator.Machine.Registers.FloatProcessor;

/// <summary>
/// A class representing a MIPS floating-point processor control register file.
/// </summary>
public class MipsFloatControlRegisterFile : RegisterFile<uint>
{
    private static readonly uint[] _maskTable;

    private const int FIR_INDEX = 0;
    private const int FCSR_INDEX = 1;

    static MipsFloatControlRegisterFile()
    {
        _maskTable = new uint[32];
        _maskTable[26] = 0x0003FC3CU; // FEXR
        _maskTable[28] = 0x01000FF3U; // FENR
        _maskTable[31] = 0x0183FFF7U; // FCSR (Master)
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsFloatControlRegisterFile"/> class.
    /// </summary>
    public MipsFloatControlRegisterFile() : base(2)
    {
    }

    /// <summary>
    /// Get or sets the FCSR register.
    /// </summary>
    public FcsrRegister FloatControlStatus
    {
        get => (FcsrRegister)this[(int)CP1CRegisters.ControlStatus];
        set => this[(int)CP1CRegisters.ControlStatus] = (uint)value;
    }

    /// <summary>
    /// Gets or sets the value in a register.
    /// </summary>
    public uint this[CP1CRegisters register]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[(int)register];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => this[(int)register] = value;
    }

    /// <inheritdoc/>
    public override uint this[int register]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => register switch
        {
            0 => _maskTable[FIR_INDEX],
            // 25 => TODO: Packed condition codes
            _ => _maskTable[FCSR_INDEX] & _maskTable[register],
        };
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            switch (register)
            {
                case 0:
                    _maskTable[FIR_INDEX] = value;
                    break;
                case 25:
                // TODO: Packed condition codes
                default:
                    var mask = _maskTable[register];
                    var fcsr = base[FCSR_INDEX] & ~mask;
                    var setter = value & mask;
                    base[FCSR_INDEX] = fcsr | setter;
                    break;
            }
        }
    }
}
