// Avishai Dernis 2026

using Zarem.Attributes.Register;

namespace Zarem.RiscV.Models.Instructions.Enums.Registers;

/// <summary>
/// An enum representing the general-purpose registers in RISC-V.
/// </summary>
public enum RiscVGpRegister : byte
{
#pragma warning disable CS1591

    [Register<RiscVRegisterCategory>("zero", RiscVRegisterCategory.Special)] Zero = 0,

    [Register<RiscVRegisterCategory>("ra", RiscVRegisterCategory.Special)] ReturnAddress = 1,
    [Register<RiscVRegisterCategory>("sp", RiscVRegisterCategory.Special)] StackPointer = 2,
    [Register<RiscVRegisterCategory>("gp", RiscVRegisterCategory.Special)] GlobalPointer = 3,
    [Register<RiscVRegisterCategory>("tp", RiscVRegisterCategory.Special)] ThreadPointer = 4,
    
    [Register<RiscVRegisterCategory>("t0", RiscVRegisterCategory.Temporary)] Temporary0 = 5,
    [Register<RiscVRegisterCategory>("t1", RiscVRegisterCategory.Temporary)] Temporary1 = 6,
    [Register<RiscVRegisterCategory>("t2", RiscVRegisterCategory.Temporary)] Temporary2 = 7,

    [Register<RiscVRegisterCategory>("s0", RiscVRegisterCategory.Saved)] Saved0 = 8,
    [Register<RiscVRegisterCategory>("fp", RiscVRegisterCategory.Special)] FramePointer = 8,
    [Register<RiscVRegisterCategory>("s1", RiscVRegisterCategory.Saved)] Saved1 = 9,
    
    [Register<RiscVRegisterCategory>("a0", RiscVRegisterCategory.Argument)] Argument0 = 10,
    [Register<RiscVRegisterCategory>("a1", RiscVRegisterCategory.Argument)] Argument1 = 11,
    [Register<RiscVRegisterCategory>("a2", RiscVRegisterCategory.Argument)] Argument2 = 12,
    [Register<RiscVRegisterCategory>("a3", RiscVRegisterCategory.Argument)] Argument3 = 13,
    [Register<RiscVRegisterCategory>("a4", RiscVRegisterCategory.Argument)] Argument4 = 14,
    [Register<RiscVRegisterCategory>("a5", RiscVRegisterCategory.Argument)] Argument5 = 15,
    [Register<RiscVRegisterCategory>("a6", RiscVRegisterCategory.Argument)] Argument6 = 16,
    [Register<RiscVRegisterCategory>("a7", RiscVRegisterCategory.Argument)] Argument7 = 17,

    [Register<RiscVRegisterCategory>("s2", RiscVRegisterCategory.Saved)] Saved2 = 18,
    [Register<RiscVRegisterCategory>("s3", RiscVRegisterCategory.Saved)] Saved3 = 19,
    [Register<RiscVRegisterCategory>("s4", RiscVRegisterCategory.Saved)] Saved4 = 20,
    [Register<RiscVRegisterCategory>("s5", RiscVRegisterCategory.Saved)] Saved5 = 21,
    [Register<RiscVRegisterCategory>("s6", RiscVRegisterCategory.Saved)] Saved6 = 22,
    [Register<RiscVRegisterCategory>("s7", RiscVRegisterCategory.Saved)] Saved7 = 23,
    [Register<RiscVRegisterCategory>("s8", RiscVRegisterCategory.Saved)] Saved8 = 24,
    [Register<RiscVRegisterCategory>("s9", RiscVRegisterCategory.Saved)] Saved9 = 25,
    [Register<RiscVRegisterCategory>("s10", RiscVRegisterCategory.Saved)] Saved10 = 26,
    [Register<RiscVRegisterCategory>("s11", RiscVRegisterCategory.Saved)] Saved11 = 27,
    
    [Register<RiscVRegisterCategory>("s3", RiscVRegisterCategory.Temporary)] Temporary3 = 28,
    [Register<RiscVRegisterCategory>("s4", RiscVRegisterCategory.Temporary)] Temporary4 = 29,
    [Register<RiscVRegisterCategory>("s5", RiscVRegisterCategory.Temporary)] Temporary5 = 30,
    [Register<RiscVRegisterCategory>("s6", RiscVRegisterCategory.Temporary)] Temporary6 = 31

#pragma warning restore CS1591
}
