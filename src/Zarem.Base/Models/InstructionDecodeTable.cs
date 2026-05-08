// Avishai Dernis 2026

using System.Runtime.CompilerServices;

namespace Zarem.Models;

/// <summary>
/// A base class for a table that decodes instructions.
/// </summary>
public abstract class InstructionDecodeTable<T, TInstruction>
{
    /// <summary>
    /// Looksup a decoded model based on the <paramref name="instruction"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract T Lookup(TInstruction instruction);
}
