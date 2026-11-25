// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Extensions;
using Zarem.Emulator.Machine.Registers;

namespace Zarem.Emulator.JIT;

/// <summary>
/// A class for managing the results of the compiler scan stage.
/// </summary>
public unsafe class JitBlockContext<T, TRegister, TTrap> : JitBlockContext<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
    where TRegister : unmanaged, Enum
    where TTrap : unmanaged, Enum
{
    private readonly Dictionary<IRegisterFile, LocalBuilder[]> _locals;
    private readonly Dictionary<IRegisterFile, HashSet<TRegister>> _loadRegisters;
    private readonly Dictionary<IRegisterFile, HashSet<TRegister>> _storeRegisters;

    /// <summary>
    /// Initializes a new instance of the <see cref="JitBlockContext{T, TRegister, TTrap}"/> class.
    /// </summary>
    public JitBlockContext(ILGenerator il, T start, T end) : base(il, start, end)
    {
        _locals = [];
        _loadRegisters = [];
        _storeRegisters = [];
    }

    /// <summary>
    /// Gets the <see cref="LocalBuilder"/> for the specified <typeparamref name="TRegister"/>.
    /// </summary>
    public LocalBuilder GetRegisterLocal(IRegisterFile file, TRegister register)
    {
        if (!_locals.TryGetValue(file, out var locals))
        {
            ThrowHelper.ThrowArgumentException<LocalBuilder>();
        }

        var index = Unsafe.As<TRegister, int>(ref register);
        return locals[index];
    }

    /// <summary>
    /// Emits the CIL to load the required registers into CIL locals.
    /// </summary>
    public void EmitSetupLocalRegisters(JitCompiler<T, TRegister, TTrap> compiler)
    {
        // For each file / usage set pair ...
        foreach (var (file, usageSet) in _loadRegisters)
        {
            // Allocate locals
            var locals = AllocateLocalRegisters(ILGenerator, file);

            // Load registers
            foreach (var reg in usageSet)
            {
                // Load register from memory into local i
                compiler.EmitStoreRegister(this, reg, il =>
                {
                    var register = reg;
                    var index = Unsafe.As<TRegister, int>(ref register);
                    compiler.EmitLoadRegisterAddress(il, index, ((IRegisterFile<T>)file).Regs);
                    il.EmitLdind<T>();
                });
            }
        }
    }

    /// <summary>
    /// Emits the CIL to flush registers from CIL locals back to the register module object.
    /// </summary>
    public void EmitFlushLocalRegisters(JitCompiler<T, TRegister, TTrap> compiler)
    {
        // For each file / usage set pair ...
        foreach (var (file, usageSet) in _storeRegisters)
        {
            // Allocate locals
            var locals = AllocateLocalRegisters(ILGenerator, file);

            // Flush registers
            foreach (var reg in usageSet)
            {
                // Load the address and value of the register
                var register = reg;
                var index = Unsafe.As<TRegister, int>(ref register);
                compiler.EmitLoadRegisterAddress(ILGenerator, index, ((IRegisterFile<T>)file).Regs);
                compiler.EmitLoadRegister<T>(this, reg);
                ILGenerator.EmitStind<T>();
            }
        }
    }

    /// <summary>
    /// Logs the loading usage of a register.
    /// </summary>
    /// <param name="file">The register file the register belongs to.</param>
    /// <param name="register">The register in the file.</param>
    public void RegisterLoadUsage(IRegisterFile file, TRegister register)
        => RegisterUsage(_loadRegisters, file, register);

    /// <summary>
    /// Logs the storage usage of a register.
    /// </summary>
    /// <param name="file">The register file the register belongs to.</param>
    /// <param name="register">The register in the file.</param>
    public void RegisterStoreUsage(IRegisterFile file, TRegister register)
        => RegisterUsage(_storeRegisters, file, register);

    /// <summary>
    /// Removes log of a register loading usage.
    /// </summary>
    /// <param name="file">The register file the register belongs to.</param>
    /// <param name="register">The register in the file.</param>
    public void UnregisterLoadUsage(IRegisterFile file, TRegister register)
        => UnregisterUsage(_loadRegisters, file, register);

    /// <summary>
    /// Removes log of a register storing usage.
    /// </summary>
    /// <param name="file">The register file the register belongs to.</param>
    /// <param name="register">The register in the file.</param>
    public void UnregisterStoreUsage(IRegisterFile file, TRegister register)
        => UnregisterUsage(_storeRegisters, file, register);

    private static void RegisterUsage(Dictionary<IRegisterFile, HashSet<TRegister>> dictionary, IRegisterFile file, TRegister register)
    {
        if (!dictionary.TryGetValue(file, out var set))
        {
            set = [];
            dictionary.Add(file, set);
        }

        set.Add(register);
    }

    private static void UnregisterUsage(Dictionary<IRegisterFile, HashSet<TRegister>> dictionary, IRegisterFile file, TRegister register)
    {
        if (!dictionary.TryGetValue(file, out var set))
            return;

        set.Remove(register);
    }

    private LocalBuilder[] AllocateLocalRegisters(ILGenerator il, IRegisterFile file)
    {
        // Setup locals
        if (!_locals.TryGetValue(file, out var locals))
        {
            locals = new LocalBuilder[file.Count];
            _locals.Add(file, locals);
        }

        // Allocate locals
        for (int i = 0; i < file.Count; i++)
        {
            locals[i] = il.DeclareLocal(file.RegisterType);
        }

        return locals;
    }
}
