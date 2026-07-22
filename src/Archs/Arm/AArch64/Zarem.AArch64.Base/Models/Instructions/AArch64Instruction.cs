// Avishai Dernis 2026

using System.Runtime.InteropServices;

namespace Zarem.AArch64.Models.Instructions;

/// <summary>
/// A struct representing an AArch64 instruction.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 4)]
public struct AArch64Instruction
{
    [FieldOffset(0)]
    private uint _inst;
}
