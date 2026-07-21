// Avishai Dernis 2025

using Zarem.Mips.Models.Instructions.Enums.Operations;

namespace Zarem.Mips.Models.Instructions.Enums.Functions.FloatProc;

/// <summary>
/// An enum for <see cref="MipsOpCode.Coprocessor1"/> instruction function codes.
/// </summary>
public enum MipsFloatFuncCode
{
#pragma warning disable CS1591

    Add = 0x00,
    Subtract = 0x01,
    Multiply = 0x02,
    Divide = 0x03,
    SquareRoot = 0x04,
    AbsoluteValue = 0x05,
    Move = 0x06,
    Negate = 0x07,
    Round_L = 0x08,
    Truncate_L = 0x09,
    Ceiling_L = 0x0a,
    Floor_L = 0x0b,
    Round_W = 0x0c,
    Truncate_W = 0x0d,
    Ceiling_W = 0x0e,
    Floor_W = 0x0f,
    Reciprical = 0x15,
    RecipricalSquareRoot = 0x16,

    ConvertToSingle = 0x20,
    ConvertToDouble = 0x21,

    ConvertToWord = 0x24,
    ConvertToLong = 0x25,
    
#pragma warning restore CS1591
}
