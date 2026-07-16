// Avishai Dernis 2024

using Zarem.Attributes.Register;

namespace Zarem.Mips.Models.Instructions.Enums.Registers;

/// <summary>
/// An enum for general process registers.
/// </summary>
public enum MipsGpRegister : byte
{
#pragma warning disable CS1591

    [Register<MipsRegisterCategory>("zero", MipsRegisterCategory.Special)] Zero = 0,
    
    [Register<MipsRegisterCategory>("at", MipsRegisterCategory.Special)] AssemblerTemporary = 1,
    [Register<MipsRegisterCategory>("v0", MipsRegisterCategory.ReturnValue)] ReturnValue0 = 2,
    [Register<MipsRegisterCategory>("v1", MipsRegisterCategory.ReturnValue)] ReturnValue1 = 3,
    
    [Register<MipsRegisterCategory>("a0", MipsRegisterCategory.Argument)] Argument0 = 4,
    [Register<MipsRegisterCategory>("a1", MipsRegisterCategory.Argument)] Argument1 = 5,
    [Register<MipsRegisterCategory>("a2", MipsRegisterCategory.Argument)] Argument2 = 6,
    [Register<MipsRegisterCategory>("a3", MipsRegisterCategory.Argument)] Argument3 = 7,
    
    [Register<MipsRegisterCategory>("t0", MipsRegisterCategory.Temporary)] Temporary0 = 8,
    [Register<MipsRegisterCategory>("t1", MipsRegisterCategory.Temporary)] Temporary1 = 9,
    [Register<MipsRegisterCategory>("t2", MipsRegisterCategory.Temporary)] Temporary2 = 10,
    [Register<MipsRegisterCategory>("t3", MipsRegisterCategory.Temporary)] Temporary3 = 11,
    [Register<MipsRegisterCategory>("t4", MipsRegisterCategory.Temporary)] Temporary4 = 12,
    [Register<MipsRegisterCategory>("t5", MipsRegisterCategory.Temporary)] Temporary5 = 13,
    [Register<MipsRegisterCategory>("t6", MipsRegisterCategory.Temporary)] Temporary6 = 14,
    [Register<MipsRegisterCategory>("t7", MipsRegisterCategory.Temporary)] Temporary7 = 15,
    
    [Register<MipsRegisterCategory>("s0", MipsRegisterCategory.Saved)] Saved0 = 16,
    [Register<MipsRegisterCategory>("s1", MipsRegisterCategory.Saved)] Saved1 = 17,
    [Register<MipsRegisterCategory>("s2", MipsRegisterCategory.Saved)] Saved2 = 18,
    [Register<MipsRegisterCategory>("s3", MipsRegisterCategory.Saved)] Saved3 = 19,
    [Register<MipsRegisterCategory>("s4", MipsRegisterCategory.Saved)] Saved4 = 20,
    [Register<MipsRegisterCategory>("s5", MipsRegisterCategory.Saved)] Saved5 = 21,
    [Register<MipsRegisterCategory>("s6", MipsRegisterCategory.Saved)] Saved6 = 22,
    [Register<MipsRegisterCategory>("s7", MipsRegisterCategory.Saved)] Saved7 = 23,
    
    [Register<MipsRegisterCategory>("t8", MipsRegisterCategory.Temporary)] Temporary8 = 24,
    [Register<MipsRegisterCategory>("t9", MipsRegisterCategory.Temporary)] Temporary9 = 25,
    
    [Register<MipsRegisterCategory>("k0", MipsRegisterCategory.Kernel)] Kernel0 = 26,
    [Register<MipsRegisterCategory>("k1", MipsRegisterCategory.Kernel)] Kernel1 = 27,
    
    [Register<MipsRegisterCategory>("gp", MipsRegisterCategory.Special)] GlobalPointer = 28,
    [Register<MipsRegisterCategory>("sp", MipsRegisterCategory.Special)] StackPointer = 29,
    [Register<MipsRegisterCategory>("fp", MipsRegisterCategory.Special)] FramePointer = 30,
    [Register<MipsRegisterCategory>("ra", MipsRegisterCategory.Special)] ReturnAddress = 31,

    // Non-indexable registers
    [Register<MipsRegisterCategory>("lo", MipsRegisterCategory.HighLow)] High = 32,
    [Register<MipsRegisterCategory>("hi", MipsRegisterCategory.HighLow)] Low = 33,

#pragma warning restore CS1591
}
