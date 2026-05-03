// Avishai Dernis 2025

using Zarem.Models.Instructions.Enums.Operations;

namespace Zarem.Models.Instructions.Enums.Functions;

/// <summary>
/// An enum for <see cref="MipsOpCode.Special3"/> instruction function codes.
/// </summary>
public enum Func3Code
{
    /// <summary>
    /// Marks that there is no function2 code.
    /// </summary>
    /// <remarks>
    /// This value is too large to encode in a real instruction. If by accident this were encoded into
    /// an <see cref="MipsInstruction"/> struct, it would become <see cref="ExtractBitField"/>.
    /// </remarks>
    None = 0x40,
    
#pragma warning disable CS1591

    ExtractBitField = 0x0,
    InsertBitField = 0x4,

    LoadWordLeftEVA = 0x19,
    LoadWordRightEVA = 0x1a,
    CacheEVA = 0x1b,
    StoreByteEVA = 0x1c,
    StoreHalfWordEVA = 0x1d,
    StoreConditionalWordEVA = 0x1e,
    StoreWordEVA = 0x1f,
    BitShiftAndFill = 0x20,
    StoreWordLeftEVA = 0x21,
    StoreWordRightEVA = 0x22,
    PrefetchEVA = 0x23,

    Cache = 0x25,
    StoreConditionalWord = 0x26,

    LoadByteUnsignedEVA = 0x28,
    LoadHalfWordUnsignedEVA = 0x29,
    LoadByteEVA = 0x2c,
    LoadHalfWordEVA = 0x2d,
    LoadLinkedWordEVA = 0x2e,
    LoadWordEVA = 0x2f,

    Prefetch = 0x35,
    LoadLinkedWord = 0x36,

    ReadHardwareRegister = 0x3b,

#pragma warning restore CS1591
}
