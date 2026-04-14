// Avishai Dernis 2026

using System.Numerics;
using System.Reflection.Emit;
using Zarem.Models.Instructions;

namespace Zarem.Emulator.JIT;

/// <summary>
/// A class which compiles blocks of MIPS code into JIT IL code.
/// </summary>
public class MipsJitCompiler<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private readonly MipsJitCpu<T> _cpu;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MipsJitCompiler{T}"/> class.
    /// </summary>
    public MipsJitCompiler(MipsJitCpu<T> cpu) => _cpu = cpu;

    /// <summary>
    /// Compiles a block of MIPS code to JIT starting at <paramref name="startPc"/>.
    /// </summary>
    /// <param name="startPc">The entry point of the JIT block.</param>
    /// <returns>The method block.</returns>
    public MipsBlockDelegate<T> CompileBlock(T startPc)
    {
        var method = new DynamicMethod($"Block_{startPc:X}", typeof(T), [typeof(MipsJitCpu<T>)], true);
        var il = method.GetILGenerator();

        T currentPc = startPc;
        bool isFinished = false;

        while (!isFinished)
        {
            var instr = (MipsInstruction)_cpu.Memory.Read<uint>(ulong.CreateTruncating(currentPc));

        }

        return (MipsBlockDelegate<T>)method.CreateDelegate(typeof(MipsBlockDelegate<T>));
    }
}
