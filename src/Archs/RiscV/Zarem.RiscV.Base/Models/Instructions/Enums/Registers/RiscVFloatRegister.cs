// Avishai Dernis 2025

using Zarem.Attributes.Register;

namespace Zarem.RiscV.Models.Instructions.Enums.Registers;

/// <summary>
/// An enum for floating-point co-processor registers.
/// </summary>
public enum RiscVFloatRegister : byte
{
#pragma warning disable CS1591

    [Register<RiscVRegisterCategory>("ft0", RiscVRegisterCategory.FloatTemporary)] Temporary0 = 0,
    [Register<RiscVRegisterCategory>("ft1", RiscVRegisterCategory.FloatTemporary)] Temporary1 = 1,
    [Register<RiscVRegisterCategory>("ft2", RiscVRegisterCategory.FloatTemporary)] Temporary2 = 2,
    [Register<RiscVRegisterCategory>("ft3", RiscVRegisterCategory.FloatTemporary)] Temporary3 = 3,
    [Register<RiscVRegisterCategory>("ft4", RiscVRegisterCategory.FloatTemporary)] Temporary4 = 4,
    [Register<RiscVRegisterCategory>("ft5", RiscVRegisterCategory.FloatTemporary)] Temporary5 = 5,
    [Register<RiscVRegisterCategory>("ft6", RiscVRegisterCategory.FloatTemporary)] Temporary6 = 6,
    [Register<RiscVRegisterCategory>("ft7", RiscVRegisterCategory.FloatTemporary)] Temporary7 = 7,
    
    [Register<RiscVRegisterCategory>("fs0", RiscVRegisterCategory.FloatSaved)] Saved0 = 8,
    [Register<RiscVRegisterCategory>("fs1", RiscVRegisterCategory.FloatSaved)] Saved1 = 9,
    
    [Register<RiscVRegisterCategory>("fa0", RiscVRegisterCategory.FloatArgument)] Argument0 = 10,
    [Register<RiscVRegisterCategory>("fa1", RiscVRegisterCategory.FloatArgument)] Argument1 = 11,
    [Register<RiscVRegisterCategory>("fa2", RiscVRegisterCategory.FloatArgument)] Argument2 = 12,
    [Register<RiscVRegisterCategory>("fa3", RiscVRegisterCategory.FloatArgument)] Argument3 = 13,
    [Register<RiscVRegisterCategory>("fa4", RiscVRegisterCategory.FloatArgument)] Argument4 = 14,
    [Register<RiscVRegisterCategory>("fa5", RiscVRegisterCategory.FloatArgument)] Argument5 = 15,
    [Register<RiscVRegisterCategory>("fa6", RiscVRegisterCategory.FloatArgument)] Argument6 = 16,
    [Register<RiscVRegisterCategory>("fa7", RiscVRegisterCategory.FloatArgument)] Argument7 = 17,
    
    [Register<RiscVRegisterCategory>("fs2", RiscVRegisterCategory.FloatArgument)] Saved2 = 18,
    [Register<RiscVRegisterCategory>("fs3", RiscVRegisterCategory.FloatArgument)] Saved3 = 19,
    [Register<RiscVRegisterCategory>("fs4", RiscVRegisterCategory.FloatArgument)] Saved4 = 20,
    [Register<RiscVRegisterCategory>("fs5", RiscVRegisterCategory.FloatArgument)] Saved5 = 21,
    [Register<RiscVRegisterCategory>("fs6", RiscVRegisterCategory.FloatArgument)] Saved6 = 22,
    [Register<RiscVRegisterCategory>("fs7", RiscVRegisterCategory.FloatArgument)] Saved7 = 23,
    [Register<RiscVRegisterCategory>("fs8", RiscVRegisterCategory.FloatArgument)] Saved8 = 24,
    [Register<RiscVRegisterCategory>("fs9", RiscVRegisterCategory.FloatArgument)] Saved9 = 25,
    [Register<RiscVRegisterCategory>("fs10", RiscVRegisterCategory.FloatArgument)] Saved10 = 26,
    [Register<RiscVRegisterCategory>("fs11", RiscVRegisterCategory.FloatArgument)] Saved11 = 27,
    
    [Register<RiscVRegisterCategory>("ft8", RiscVRegisterCategory.FloatTemporary)] Temporary8 = 28,
    [Register<RiscVRegisterCategory>("ft9", RiscVRegisterCategory.FloatTemporary)] Temporary9 = 29,
    [Register<RiscVRegisterCategory>("ft10", RiscVRegisterCategory.FloatTemporary)] Temporary10 = 30,
    [Register<RiscVRegisterCategory>("ft11", RiscVRegisterCategory.FloatTemporary)] Temporary11 = 31,

#pragma warning restore CS1591
}
