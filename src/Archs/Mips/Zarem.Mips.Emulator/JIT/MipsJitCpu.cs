// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.CompilerServices;
using Zarem.Emulator.JIT;
using Zarem.Emulator.Machine.Memory;
using Zarem.Emulator.Models.JIT;
using Zarem.Mips.Emulator.Config;
using Zarem.Mips.Emulator.Interpret.CoProcessors.FloatCoProcessor;
using Zarem.Mips.Emulator.JIT.CoProcessors;
using Zarem.Mips.Emulator.JIT.CoProcessors.FloatCoProcessor;
using Zarem.Mips.Emulator.Machine;
using Zarem.Mips.Emulator.Machine.CoProcessors;
using Zarem.Mips.Emulator.Machine.Enums;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Models.Versioning.Enums;

namespace Zarem.Mips.Emulator.JIT;

/// <summary>
/// A <see cref="MipsCpu{T}"/> which uses JIT cross-compilation for execution.
/// </summary>
public partial class MipsJitCpu<T> : MipsCpu<T>
    where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
{
}
