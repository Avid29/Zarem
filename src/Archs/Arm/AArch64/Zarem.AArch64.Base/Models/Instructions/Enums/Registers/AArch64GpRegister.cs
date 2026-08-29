// Avishai Dernis 2026

using Zarem.Attributes.Register;

namespace Zarem.AArch64.Models.Instructions.Enums.Registers;

/// <summary>
/// An enum for AArch64 general process registers.
/// </summary>
public enum AArch64GpRegister : byte
{
#pragma warning disable CS1591

    // Arguments and Return Values
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Argument)] X0 = 0,
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Argument)] X1 = 1,
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Argument)] X2 = 2,
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Argument)] X3 = 3,
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Argument)] X4 = 4,
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Argument)] X5 = 5,
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Argument)] X6 = 6,
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Argument)] X7 = 7,

    // Caller-saved / Temporary Registers
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.IndirectResult)] X8 = 8,
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Temporary)] X9 = 9,
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Temporary)] X10 = 10,
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Temporary)] X11 = 11,
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Temporary)] X12 = 12,
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Temporary)] X13 = 13,
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Temporary)] X14 = 14,
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Temporary)] X15 = 15,

    // Intra-Procedure-Call / Temporary Registers
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Temporary)] X16 = 16,
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Temporary)] X17 = 17,
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Platform)] X18 = 18,

    // Callee-saved Registers
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Saved)] X19 = 19,
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Saved)] X20 = 20,
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Saved)] X21 = 21,
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Saved)] X22 = 22,
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Saved)] X23 = 23,
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Saved)] X24 = 24,
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Saved)] X25 = 25,
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Saved)] X26 = 26,
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Saved)] X27 = 27,
    [Register<AArch64RegisterCategory>(AArch64RegisterCategory.Saved)] X28 = 28,

    // Special Purpose Architectural Registers
    [Register<AArch64RegisterCategory>("fp", AArch64RegisterCategory.Special)] FramePointer = 29,
    [Register<AArch64RegisterCategory>("lr", AArch64RegisterCategory.Special)] LinkRegister = 30,
    
    // Zero register
    [Register<AArch64RegisterCategory>("xzr", AArch64RegisterCategory.Special)] Zero = 31,

    // Stack Pointer (Distinct from XZR, shares hardware encoding 31 contextually)
    [Register<AArch64RegisterCategory>("sp", AArch64RegisterCategory.Special)] StackPointer = 31,
#pragma warning restore CS1591
}
