// Avishai Dernis 2026

using System.Numerics;
using System.Threading;
using Zarem.Emulator.Config;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Machine.Enums;
using Zarem.Models.Instructions;

namespace Zarem.Emulator.JIT;

/// <summary>
/// A <see cref="RiscVJitCpu{T}"/> which uses JIT cross-compilation for execution.
/// </summary>
public class RiscVJitCpu<T> : RiscVCpu<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
    private readonly JitBlockCache<T, RiscVJitBlock<T>> _blockCache;
    private readonly RiscVJitCompiler<T> _jitCompiler;

    /// <summary>
    /// Initializes a new instance of the <see cref="RiscVCpu{T}"/> class.
    /// </summary>
    public RiscVJitCpu(RiscVEmulatorConfig config, PhysicalBus bus) : base(config, bus)
    {
        _blockCache = new();
        _jitCompiler = new RiscVJitCompiler<T>(this);
    }

    /// <inheritdoc/>
    public override void Insert(RiscVInstruction instruction, out RiscVTrap trap)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public override void Run(CancellationToken ct)
    {
        throw new System.NotImplementedException();
    }
}
