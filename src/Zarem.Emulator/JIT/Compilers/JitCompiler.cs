// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Extensions;
using Zarem.Emulator.Machine.CPU;
using Zarem.Emulator.Machine.Memory;
using Zarem.Emulator.Machine.Registers;
using Zarem.Emulator.Models.Enums;

namespace Zarem.Emulator.JIT.Compilers;

/// <summary>
/// A base class for a component which recompiles blocks of assembly into .NET CIL code.
/// </summary>
public abstract partial class JitCompiler<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JitCompiler{T, TRegister, TTrap}"/> class.
    /// </summary>
    public JitCompiler(ICpu cpu)
    {
        var getMemoryMethod = cpu.GetType().GetProperty(nameof(ICpu.Memory))?.GetGetMethod();
        Guard.IsNotNull(getMemoryMethod);
        GetMemoryMethod = getMemoryMethod;

        Type[] readWritetypes = [typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint)];
        foreach (var type in readWritetypes)
        {
            TryReadMethods[type] = typeof(MemorySystem)
                .GetMethods()
                .First(m => m.Name == nameof(MemorySystem.TryRead) && m.IsGenericMethod && m.GetParameters().Length is 2)
                .MakeGenericMethod(type);
            TryWriteMethods[type] = typeof(MemorySystem)
                .GetMethods()
                .First(m => m.Name == nameof(MemorySystem.TryWrite) && m.IsGenericMethod && m.GetParameters().Length is 2)
                .MakeGenericMethod(type);
        }
    }

    /// <summary>
    /// Gets the cpu the JIT compiler operates on.
    /// </summary>
    public abstract ICpu Cpu { get; }

    /// <summary>
    /// Gets a <see cref="MethodInfo"/> for retreiving the <see cref="ICpu.Memory"/> for the compiler.
    /// </summary>
    protected MethodInfo GetMemoryMethod { get; }

    /// <summary>
    /// Gets a <see cref="Dictionary{Type, MethodInfo}"/> for looking up memory read methods by type.
    /// </summary>
    protected Dictionary<Type, MethodInfo> TryReadMethods { get; } = [];

    /// <summary>
    /// Gets a <see cref="Dictionary{Type, MethodInfo}"/> for looking up memory write methods by type.
    /// </summary>
    protected Dictionary<Type, MethodInfo> TryWriteMethods { get; } = [];

    /// <summary>
    /// Compiles a block of MIPS code to JIT starting at <paramref name="startPc"/>.
    /// </summary>
    /// <param name="startPc">The entry point of the JIT block.</param>
    /// <returns>The method block.</returns>
    public abstract JitBlock<T> CompileBlock(T startPc);
}
