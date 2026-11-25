// Avishai Dernis 2026

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
public unsafe class JitBlockContext<T, TRegister, TTrap>
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
    public JitBlockContext()
    {
        _locals = [];
        _loadRegisters = [];
        _storeRegisters = [];
    }

    /// <summary>
    /// Emits the CIL to load the required registers into CIL locals.
    /// </summary>
    public void EmitSetupLocalRegisters(ILGenerator il, JitCompiler<T, TRegister, TTrap> compiler)
    {
        // For each register file / usage set pair ...
        foreach (var (registerFile, usageSet) in _loadRegisters)
        {
            // Allocate locals
            var locals = AllocateLocalRegisters(il, registerFile);

            // Load registers
            foreach (var reg in usageSet)
            {
                // Load register from memory into local i
                compiler.EmitStoreRegister(il, reg, il =>
                {
                    var register = reg;
                    var index = Unsafe.As<TRegister, int>(ref register);
                    compiler.EmitLoadRegisterAddress(il, index, ((IRegisterFile<T>)registerFile).Regs);
                    il.EmitLdind<T>();
                });
            }
        }
    }

    /// <summary>
    /// Emits the CIL to flush registers from CIL locals back to the register module object.
    /// </summary>
    public void EmitFlushLocalRegisters(ILGenerator il, JitCompiler<T, TRegister, TTrap> compiler)
    {
        // For each register file / usage set pair ...
        foreach (var (registerFile, usageSet) in _storeRegisters)
        {
            // Allocate locals
            var locals = AllocateLocalRegisters(il, registerFile);

            // Flush registers
            foreach (var reg in usageSet)
            {
                // Load the address and value of the register
                var register = reg;
                var index = Unsafe.As<TRegister, int>(ref register);
                compiler.EmitLoadRegisterAddress(il, index, ((IRegisterFile<T>)registerFile).Regs);
                compiler.EmitLoadRegister<T>(il, reg);
                il.EmitStind<T>();
            }
        }
    }

    private LocalBuilder[] AllocateLocalRegisters(ILGenerator il, IRegisterFile registerFile)
    {
        // Setup locals
        if (!_locals.TryGetValue(registerFile, out var locals))
        {
            locals = new LocalBuilder[registerFile.Count];
        }

        // Allocate locals
        for (int i = 0; i < registerFile.Count; i++)
        {
            locals[i] = il.DeclareLocal(registerFile.RegisterType);
        }

        _locals[registerFile] = locals;
        return locals;
    }
}
