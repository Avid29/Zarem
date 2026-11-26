// Avishai Dernis 2026

using System.Numerics;

namespace Zarem.Emulator.JIT.Compilers;

/// <summary>
/// A base class for an auxilary component which helps a <see cref="JitCompiler{T, TRegister, TTrap}"/> recompile blocks of assembly into .NET CIL code.
/// </summary>
public abstract partial class JitChildCompiler<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JitChildCompiler{T, TRegister, TTrap}"/> class.
    /// </summary>
    public JitChildCompiler()
    {
    }
}
