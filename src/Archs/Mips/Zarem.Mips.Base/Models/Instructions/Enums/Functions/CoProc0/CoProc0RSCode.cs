// Avishai Dernis 2025

using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Instructions.Enums.Operations;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.Mips.Models.Instructions.Enums.Functions.CoProc0;

/// <summary>
/// An enum for <see cref="MipsOpCode.Coprocessor0"/> instruction rs field function codes.
/// </summary>
public enum CoProc0RSCode : byte
{
    /// <summary>
    /// Marks that there is no RS code.
    /// </summary>
    /// <remarks>
    /// This value is too large to encode in a real instruction. If by accident
    /// this were encoded into an <see cref="MipsInstruction"/> struct, it would become 
    /// <see cref="MFC0"/> (or <see cref="MipsGpRegister.Zero"/>) upon unencoding.
    /// </remarks>
    None = 0x20,

    #pragma warning disable CS1591

    MFC0 = 0x0,
    MFHC0 = 0x2,
    MTC0 = 0x4,
    MTHC0 = 0x6,
    MFMC0 = 0xb,
    RDPGPR = 0xa,
    WRPGPR = 0xe,
    C0 = 0x10,

    #pragma warning restore CS1591
}
