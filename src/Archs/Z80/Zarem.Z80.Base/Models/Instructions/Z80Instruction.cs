// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Zarem.Helpers;
using Zarem.Z80.Models.Instructions.Enums.Operations;
using Zarem.Z80.Models.Instructions.Enums.Registers;

namespace Zarem.Z80.Models.Instructions;

/// <summary>
/// A struct representing a Z80 instruction.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct Z80Instruction
{
    // The unified raw instruction stream buffer
    [FieldOffset(0)] private ulong _inst;

    // Fast tracking metadata
    [FieldOffset(6)] private byte _prefixLength;   // Cached prefix length
    [FieldOffset(7)] private byte _length;         // Cached decoded length (1 to 5)

    /// <summary>
    /// Gets the length of this specific instruction execution instance.
    /// </summary>
    public readonly int Length => _length;

    /// <summary>
    /// Gets or sets the instruction prefix.
    /// </summary>
    public Z80Prefix Prefix
    {
        readonly get
        {
            return _prefixLength switch
            {
                0 => Z80Prefix.None,
                1 => (Z80Prefix)(_inst & byte.MaxValue),
                2 => (Z80Prefix)(_inst & ushort.MaxValue),
                _ => ThrowHelper.ThrowArgumentException<Z80Prefix>(),
            };
        }
        set
        {
            ushort newPrefixVal = (ushort)value;
            byte newPrefixLen = newPrefixVal switch
            {
                0 => 0,
                <= byte.MaxValue => 1,
                _ => 2
            };

            // If the layout context is not changing, we can fast-path update the prefix bits directly
            if (_prefixLength == newPrefixLen)
            {
                if (newPrefixLen == 1) BitField.SetField(ref _inst, 8, 0, newPrefixVal);
                else if (newPrefixLen == 2) BitField.SetField(ref _inst, 16, 0, newPrefixVal);
                return;
            }

            // Preserve the semantic payloads before the layout changes
            // Note: Opcode needs careful isolation since its structural byte position transforms
            Z80OpCode oldOpCode = OpCode;
            sbyte oldDisp = Displacement;
            ushort oldImm16 = Immediate16;

            // Commit the new layout tracking footprint configuration
            _prefixLength = newPrefixLen;

            if (newPrefixLen == 0) _inst = 0;
            else if (newPrefixLen == 1) BitField.SetField(ref _inst, 8, 0, newPrefixVal);
            else BitField.SetField(ref _inst, 16, 0, newPrefixVal);

            // Restore the semantic data components back to their newly computed relative bit alignments
            OpCode = oldOpCode;
            Displacement = oldDisp;
            Immediate16 = oldImm16;
        }
    }

    /// <summary>
    /// Gets or sets the instruction's op code.
    /// </summary>
    public Z80OpCode OpCode
    {
        readonly get
        {
            byte byte0 = (byte)(_inst & byte.MaxValue);
            byte byte3 = (byte)((_inst >> sizeof(ushort)) & byte.MaxValue);
            ushort lower = (ushort)(_inst & ushort.MaxValue);

            return _prefixLength switch
            {
                0 => (Z80OpCode)byte0,
                1 => (Z80OpCode)lower,
                2 => (Z80OpCode)((lower << 8) | byte3),
                _ => ThrowHelper.ThrowArgumentException<Z80OpCode>(),
            };
        }
        set
        {
            uint val = (uint)value;
            byte targetPrefixLen = val switch
            {
                <= byte.MaxValue => 0,
                <= ushort.MaxValue => 1,
                _ => 2,
            };

            // If the incoming opcode implies a prefix change, migrate the entire layout first
            if (targetPrefixLen is not 0 || _prefixLength != targetPrefixLen)
                Prefix = (Z80Prefix)(val & ushort.MaxValue);

            // Set the opcode
            SetField(8, 0, (byte)value);
        }
    }

    /// <summary>
    /// Gets or sets the instruction's register argument.
    /// </summary>
    public Z80Register Register
    {
        readonly get => (Z80Register)GetField(3, 5);
        set => SetField(3, 5, (byte)value);
    }

    /// <summary>
    /// Gets or sets the immediate 8-bit operand.
    /// </summary>
    public byte Immediate8
    {
        readonly get => (byte)GetField(8, 8);
        set => SetField(8, 8, value);
    }

    /// <summary>
    /// Gets or sets the immediate 16-bit operand.
    /// </summary>
    public ushort Immediate16
    {
        readonly get => GetField(16, 0);
        set => SetField(16, 0, value);
    }

    /// <summary>
    /// Gets or sets the Z80 indexed displacement byte (d).
    /// </summary>
    public sbyte Displacement
    {
        readonly get
        {
            // For a 2-byte prefix, displacement is strictly locked at hardware _byte2 (bit-shift 16)
            if (_prefixLength == 2)
            {
                return (sbyte)BitField.GetField(_inst, 8, 16);
            }

            return (sbyte)GetField(8, 0);
        }
        set
        {
            // For a 2-byte prefix, displacement is strictly locked at hardware _byte2 (bit-shift 16)
            if (_prefixLength == 2)
            {
                BitField.SetField(ref _inst, 8, 16, (byte)value);
            }
            else
            {
                SetField(8, 0, (byte)value);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly ushort GetField(int size, int relativeOffset)
        => (ushort)BitField.GetField(_inst, size, GetOffset(relativeOffset));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetField(int size, int relativeOffset, ushort value)
        => BitField.SetField(ref _inst, size, GetOffset(relativeOffset), value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly int GetOffset(int relativeOffset)
    {
        int baseShift = _prefixLength switch
        {
            0 => 0,
            1 => 1,
            2 => 3, // Skip the displacement.
            _ => ThrowHelper.ThrowArgumentException<int>(),
        };

        baseShift *= sizeof(byte);
        return baseShift + relativeOffset;
    }
}
