// Avishai Dernis 2026

using System.Reflection.Emit;

namespace Zarem.Emulator.JIT;

/// <summary>
/// A base class for a component which recompiles blocks of assembly into .NET CIL code.
/// </summary>
public partial class JitCompiler<T, TRegister, TTrap>
{
    /// <summary>
    /// Discovers a JIT block.
    /// </summary>
    /// <param name="il">The <see cref="ILGenerator"/> for the block.</param>
    /// <param name="startPc">The start address of the block.</param>
    /// <returns>The JIT Block.</returns>
    protected abstract JitBlockContext<T, TRegister, TTrap> DiscoverBlock(ILGenerator il, T startPc);

    /// <summary>
    /// Scans register usage in a JIT block.
    /// </summary>
    protected abstract void ScanRegisterUsage(JitBlockContext<T, TRegister, TTrap> context);
}
