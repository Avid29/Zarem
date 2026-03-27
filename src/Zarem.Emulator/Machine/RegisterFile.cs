// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Zarem.Emulator.Machine;

/// <summary>
/// A register file.
/// </summary>
public unsafe class RegisterFile<T> : IDisposable
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterFile{T}"/> class.
    /// </summary>
    public RegisterFile(int count)
    {
        Regs = (T*)NativeMemory.AllocZeroed((nuint)count, (nuint)sizeof(T));
        Count = count;
    }

    /// <summary>
    /// Gets the number of registers in the register file.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Gets an unsafe point to the registers
    /// </summary>
    public T* Regs { get; }

    /// <summary>
    /// Gets or sets the value in a register.
    /// </summary>
    public virtual T this[int register]
    {
        get
        {
#if DEBUG
            Guard.IsBetween(register, 0, Count);
#endif
            return Regs[register];
        }
        set
        {
#if DEBUG
            Guard.IsBetween(register, 0, Count);
#endif
            Regs[register] = value;
        }
    }

    /// <inheritdoc/>
    public void Dispose() => NativeMemory.Free(Regs);
}
