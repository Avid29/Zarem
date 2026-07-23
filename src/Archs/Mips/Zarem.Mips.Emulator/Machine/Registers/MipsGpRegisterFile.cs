// Avishai Dernis 2025

using CommunityToolkit.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Machine.Registers;
using Zarem.Mips.Models.Versioning;
using Zarem.Mips.Models.Versioning.Enums;

namespace Zarem.Mips.Emulator.Machine.Registers;

/// <summary>
/// A class representing a register file.
/// </summary>
public unsafe class MipsGPRegisterFile<T> : RegisterFile<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MipsGPRegisterFile{T}"/> class.
    /// </summary>
    public MipsGPRegisterFile(MipsVersionInfo versionInfo) :
        base(versionInfo.Base is < MipsBaseVersion.R6 ? 34 : 32)
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

    /// <summary>
    /// Gets or sets the high register.
    /// </summary>
    /// <remarks>
    /// Will out of bound in MIPS32/64 Release 6 resulting in undefined behavior 
    /// </remarks>
    public T High
    {
        get
        {
#if DEBUG
            Guard.IsGreaterThan(Count, 32);
#endif 
            return Regs[32];
        }
        set
        {
#if DEBUG
            Guard.IsGreaterThan(Count, 32);
#endif 
            Regs[32] = value;
        }
    }

    /// <summary>
    /// Gets or sets the low register.
    /// </summary>
    /// <remarks>
    /// Will out of bound in MIPS32/64 Release 6 resulting in undefined behavior 
    /// </remarks>
    public T Low
    {
        get
        {
#if DEBUG
            Guard.IsGreaterThan(Count, 32);
#endif 
            return Regs[33];
        }
        set
        {
#if DEBUG
            Guard.IsGreaterThan(Count, 33);
#endif 
            Regs[33] = value;
        }
    }
}
