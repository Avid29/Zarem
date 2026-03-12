// Avishai Dernis 2026

using Zarem.Debugger.Handlers;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums.Registers;
using Zarem.Models.Instructions.Enums.SpecialFunctions;

namespace Zarem.Debugger.MIPS;

/// <summary>
/// A <see cref="IDebugHandler"/> for the mips architecture.
/// </summary>
public class MipsDebugHandler : IDebugHandler
{
    private readonly byte[] _breakPointBytes;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsDebugHandler"/> class.
    /// </summary>
    public MipsDebugHandler()
    {
        var breakInstruction = MipsInstruction.Create(FunctionCode.Break, GPRegister.Zero, GPRegister.Zero, GPRegister.Zero);
        _breakPointBytes = BitConverter.GetBytes((uint)breakInstruction);

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(_breakPointBytes);
        }
    }

    /// <inheritdoc/>
    public ReadOnlySpan<byte> BreakpointBytes => _breakPointBytes;

    /// <inheritdoc/>
    public uint InstructionSize => 4;
}
