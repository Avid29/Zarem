// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Zarem.Emulator.Extensions;
using Zarem.Emulator.JIT.Compilers;
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
    private record struct RegisterFileTracker(
        LocalBuilder[] Locals,
        HashSet<TRegister> LoadRegisters,
        HashSet<TRegister> DirtyRegister);

    private readonly Dictionary<IRegisterFile, RegisterFileTracker> _fileTrackers;
    private readonly Label _setupLabel;
    private readonly Label _bodyLabel;

    /// <summary>
    /// Initializes a new instance of the <see cref="JitBlockContext{T, TRegister, TTrap}"/> class.
    /// </summary>
    public JitBlockContext(ILGenerator il, T start) : base(il, start)
    {
        _fileTrackers = [];

        // Method layout structure:
        // 0: [Entry]       -> Jumps forward to [Setup]
        // 1: [Body]        -> Hot instruction stream (registers accumulate lazily here)
        // 2: [Exit]        -> Flushes dirty registers back to physical memory and returns
        // 3: [Setup]       -> Emits lazy register loads now that all used registers are known
        // 4: [SetupDone]   -> Jumps back into the start of [Body]
        //
        // This deferred layout allows register usage to accumulate organically 
        // while parsing the body before generating the entry setup and exit flush code.

        // NOTE: Currently, traps create additional exit sections.

        // Declare jump labels
        _setupLabel = il.DefineLabel();
        _bodyLabel = il.DefineLabel();

        // Emit Entry section
        il.Emit(OpCodes.Br, _setupLabel);
        il.MarkLabel(_bodyLabel);
    }

    /// <summary>
    /// Gets the <see cref="LocalBuilder"/> for the specified <typeparamref name="TRegister"/>.
    /// </summary>
    public LocalBuilder GetOrCreateRegisterLocal(IRegisterFile file, TRegister register, bool modify)
    {
        if (!_fileTrackers.TryGetValue(file, out var tracker))
        {
            tracker = new()
            {
                Locals = new LocalBuilder[file.Count],
                LoadRegisters = [],
                DirtyRegister = [],
            };

            for (int i = 0; i < file.Count; i++)
            {
                tracker.Locals[i] = ILGenerator.DeclareLocal(file.RegisterType);
            }

            _fileTrackers.Add(file, tracker);
        }

        // Log register access for setup and/or flush
        var logger = modify ? tracker.DirtyRegister : tracker.LoadRegisters;
        logger.Add(register);

        var index = Unsafe.As<TRegister, int>(ref register);
        return tracker.Locals[index];
    }

    /// <summary>
    /// Emits the CIL to load the required registers into CIL locals.
    /// </summary>
    public void EmitSetupLocalRegisters(JitCompiler<T, TRegister, TTrap> compiler)
    {
        // Mark the setup section
        ILGenerator.MarkLabel(_setupLabel);

        // For each file / usage set pair ...
        foreach (var (file, tracker) in _fileTrackers)
        {
            // Load registers
            foreach (var reg in tracker.LoadRegisters)
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

        // Finalize setup by jumping back into the body
        ILGenerator.Emit(OpCodes.Br, _bodyLabel);
    }

    /// <summary>
    /// Emits the CIL to flush registers from CIL locals back to the register module object.
    /// </summary>
    public void EmitFlushLocalRegisters(JitCompiler<T, TRegister, TTrap> compiler)
    {
        // For each file / usage set pair ...
        foreach (var (file, tracker) in _fileTrackers)
        {
            // Flush registers
            foreach (var reg in tracker.DirtyRegister)
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
}
