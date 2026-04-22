// Avishai Dernis 2026

using System.Reflection.Emit;

namespace Zarem.Emulator.JIT;

/// <summary>
/// A class which compiles blocks of RISC-V code into JIT IL code.
/// </summary>
public partial class RiscVJitCompiler<T>
{
    /// <inheritdoc/>
    protected override void EmitSetupLocalRegisters(ILGenerator il) => EmitSetupLocalRegisters(il, _cpu.RegisterFile, _loadRegs);

    /// <inheritdoc/>
    protected override void EmitFlushLocalRegisters(ILGenerator il) => EmitFlushLocalRegisters(il, _cpu.RegisterFile, _storeRegs);
}
