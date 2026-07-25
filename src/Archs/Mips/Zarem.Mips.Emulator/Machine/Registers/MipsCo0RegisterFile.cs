// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Machine.Registers;
using Zarem.Mips.Emulator.Machine.Tlb;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Mips.Emulator.Machine.Registers;

/// <summary>
/// A class representing a MIPS register file.
/// </summary>
public class MipsCo0RegisterFile<T> : RegisterFile<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private readonly int _maxTlbIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsCo0RegisterFile{T}"/> class.
    /// </summary>
    public MipsCo0RegisterFile(int tlbSlotCount) : base(32)
    {
        _maxTlbIndex = tlbSlotCount - 1;
    }

    /// <summary>
    /// Gets the <see cref="CP0Registers.Random"/> register value.
    /// </summary>
    public int Random
    {
        get
        {
            // If Wired is out of bounds or only allows the final slot
            // just return the final slot.
            if (Wired >= _maxTlbIndex)
            {
                return _maxTlbIndex;
            }

            // Pick a random slot in range
            return System.Random.Shared.Next(Wired, _maxTlbIndex + 1);
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="CP0Registers.EntryLo0"/> register value.
    /// </summary>
    public MipsTlbEntryLow<T> EntryLow0
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (MipsTlbEntryLow<T>)this[(int)CP0Registers.EntryLo0];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => this[(int)CP0Registers.EntryLo0] = value;
    }

    /// <summary>
    /// Gets or sets the <see cref="CP0Registers.EntryLo1"/> register value.
    /// </summary>
    public MipsTlbEntryLow<T> EntryLow1
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (MipsTlbEntryLow<T>)this[(int)CP0Registers.EntryLo1];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => this[(int)CP0Registers.EntryLo1] = value;
    }

    /// <summary>
    /// Gets or sets the <see cref="CP0Registers.BadVAddr"/> register value.
    /// </summary>
    public T BadVirtualAddress
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[(int)CP0Registers.BadVAddr];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => this[(int)CP0Registers.BadVAddr] = value;
    }

    /// <summary>
    /// Gets or sets the <see cref="CP0Registers.Wired"/> register value.
    /// </summary>
    public int Wired
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => int.CreateTruncating(this[(int)CP0Registers.Wired]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => this[(int)CP0Registers.Wired] = T.CreateTruncating(value);
    }

    /// <summary>
    /// Gets or sets the <see cref="CP0Registers.EntryHi"/> register value.
    /// </summary>
    public MipsTlbEntryHigh<T> EntryHi
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (MipsTlbEntryHigh<T>)this[(int)CP0Registers.EntryHi];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => this[(int)CP0Registers.EntryHi] = value;
    }

    /// <summary>
    /// Gets or sets the <see cref="CP0Registers.Status"/> register value.
    /// </summary>
    public StatusRegister StatusRegister
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (StatusRegister)uint.CreateTruncating(this[(int)CP0Registers.Status]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => this[(int)CP0Registers.Status] = T.CreateTruncating((uint)value);
    }

    /// <summary>
    /// Gets or sets the <see cref="CP0Registers.Cause"/> register value.
    /// </summary>
    public CauseRegister CauseRegister
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (CauseRegister)uint.CreateTruncating(this[(int)CP0Registers.Cause]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => this[(int)CP0Registers.Cause] = T.CreateTruncating((uint)value);
    }

    /// <summary>
    /// Gets or sets the raw data at the specified register index location, 
    /// intercepting hardware-managed behavior properties.
    /// </summary>
    public override T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            // Override Random 
            if (index == (int)CP0Registers.Random)
                return T.CreateTruncating(Random);

            return base[index];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            // According to MIPS hardware specification, writing to the Random slot 
            // is architecturally ignored or undefined behavior.
            if (index == (int)CP0Registers.Random)
                return;

            base[index] = value;
        }
    }
}
