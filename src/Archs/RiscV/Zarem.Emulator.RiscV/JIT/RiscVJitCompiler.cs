// Avishai Dernis 2026

using System.Collections.Generic;
using System.Numerics;
using Zarem.Emulator.Machine.Enums;
using Zarem.Models.Instructions.Enums.Registers;

namespace Zarem.Emulator.JIT;

/// <summary>
/// A class which compiles blocks of RISC-V code into JIT IL code.
/// </summary>
public partial class RiscVJitCompiler<T> : JitCompiler<T, RiscVGpRegister, RiscVTrap>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private readonly RiscVJitCpu<T> _cpu;

    private readonly HashSet<RiscVGpRegister> _loadRegs = [];
    private readonly HashSet<RiscVGpRegister> _storeRegs = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVJitCompiler{T}"/> class.
    /// </summary>
    public RiscVJitCompiler(RiscVJitCpu<T> cpu)
    {
        _cpu = cpu;
    }
}
