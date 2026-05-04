// Avishai Dernis 2025

using Zarem.Mips.Models.Instructions.Enums.Operations;

namespace Zarem.Mips.Models.Instructions.Enums.Functions.CoProc0;

/// <summary>
/// An enum for <see cref="MipsOpCode.Coprocessor0"/> instruction function codes.
/// </summary>
public enum Co0FuncCode
{
    /// <summary>
    /// Marks that there is no coproc0 function code.
    /// </summary>
    /// <remarks>
    /// This value is too large to encode in a real instruction. If by accident this were encoded into
    /// an <see cref="MipsInstruction"/> struct, it would become 0.
    /// </remarks>
    None = 0x40,

#pragma warning disable CS1591

    ReadIndexedTLBEntry = 0x1,
    WriteIndexedTLBEntry = 0x2,

    TLBInvalidate = 0x3,
    TLBInvalidateFlush = 0x4,

    WriteRandomTLBEntry = 0x6,

    ProbeTLBForMatch = 0x8,

    ExceptionReturn = 0x18,
    DebugExceptionReturn = 0x1f,

    Wait = 0x20,

#pragma warning restore CS1591
}
