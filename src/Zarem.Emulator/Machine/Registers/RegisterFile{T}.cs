// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Zarem.Emulator.Machine.Registers;

/// <summary>
/// A register file.
/// </summary>
public unsafe class RegisterFile<T> : IRegisterFile, IDisposable
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
    /// Gets an unsafe point to the registers
    /// </summary>
    public T* Regs { get; }

    /// <inheritdoc/>
    public int Count { get; }

    /// <summary>
    /// Gets or sets the value in a register.
    /// </summary>
    public virtual T this[int register]
    {
        get
        {
#if DEBUG
            Guard.IsInRange(register, 0, Count);
#endif
            return Regs[register];
        }
        set
        {
#if DEBUG
            Guard.IsInRange(register, 0, Count);
#endif
            Regs[register] = value;
        }
    }

    /// <inheritdoc/>
    ulong IRegisterFile.this[int register]
    {
        get => ulong.CreateTruncating(this[register]);
        set => this[register] = T.CreateTruncating(value);
    }

    /// <inheritdoc/>
    public void Dispose() => NativeMemory.Free(Regs);
}
