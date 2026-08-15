// Avishai Dernis 2026

using Zarem.Attributes.Register;

namespace Zarem.RiscV.Models.Instructions.Enums.Registers;

/// <summary>
/// An enum representing the compressed general-purpose registers in RISC-V.
/// </summary>
public enum RiscVCompressedGpRegister : byte
{
#pragma warning disable CS1591

    [Register<RiscVRegisterCategory>("s0", RiscVRegisterCategory.Saved)] Saved0 = 0,
    [Register<RiscVRegisterCategory>("fp", RiscVRegisterCategory.Special)] FramePointer = 0,
    [Register<RiscVRegisterCategory>("s1", RiscVRegisterCategory.Saved)] Saved1 = 1,
    
    [Register<RiscVRegisterCategory>("a0", RiscVRegisterCategory.Argument)] Argument0 = 2,
    [Register<RiscVRegisterCategory>("a1", RiscVRegisterCategory.Argument)] Argument1 = 3,
    [Register<RiscVRegisterCategory>("a2", RiscVRegisterCategory.Argument)] Argument2 = 4,
    [Register<RiscVRegisterCategory>("a3", RiscVRegisterCategory.Argument)] Argument3 = 5,
    [Register<RiscVRegisterCategory>("a4", RiscVRegisterCategory.Argument)] Argument4 = 6,
    [Register<RiscVRegisterCategory>("a5", RiscVRegisterCategory.Argument)] Argument5 = 7,

#pragma warning restore CS1591
}
