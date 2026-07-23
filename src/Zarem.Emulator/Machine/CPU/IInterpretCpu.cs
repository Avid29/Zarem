// Avishai Dernis 2026

using System;

namespace Zarem.Emulator.Machine.CPU;

/// <summary>
/// An interface for an interpret-emulation-based CPU.
/// </summary>
public interface IInterpretCpu<TSelf, TInstruction, TExecution, TTrap> : ICpu<TSelf, TInstruction, TTrap>
    where TSelf : IInterpretCpu<TSelf, TInstruction, TExecution, TTrap>
    where TTrap : Enum 
{
    /// <inheritdoc cref="ICpu{TSelf, TInstruction, TTrap}.Insert(TInstruction, out TTrap)"/>
    void Insert(TInstruction instruction, out TExecution exec, out TTrap trap);
}
