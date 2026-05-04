// Avishai Dernis 2025

using System.Numerics;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Machine;

namespace Zarem.RiscV.Emulator.Machine.Registers;

/// <summary>
/// A class representing a register file.
/// </summary>
public class RiscVGPRegisterFile<T> : RegisterFile<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVGPRegisterFile{T}"/> class.
    /// </summary>
    public RiscVGPRegisterFile() : base(32)
    {
    }

    /// <summary>
    /// Gets or sets the value in a register.
    /// </summary>
    public override T this[int register]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => base[register];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            // Cannot set the 0 GPR register
            if (register is 0)
                return;

            base[register] = value;
        }
    }
}
