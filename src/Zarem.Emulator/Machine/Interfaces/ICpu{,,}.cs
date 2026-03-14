// Avishai Dernis 2026

using System;

namespace Zarem.Emulator.Machine.Interfaces;

/// <summary>
/// An interface for a cpu in an emulated machine.
/// </summary>
public interface ICpu<TSelf, TInstruction, TTrap> : ICpu
    where TSelf : ICpu<TSelf, TInstruction, TTrap>
    where TTrap : Enum
{
    /// <summary>
    /// Advances the state of the emulator by one step.
    /// </summary>
    void Step();

    /// <summary>
    /// Executes an instruction on the current state of the processor.
    /// </summary>
    public void Insert(TInstruction instruction, out TTrap trap);
}
