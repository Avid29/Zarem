// Avishai Dernis 2026

namespace Zarem.Debugger.Models.Enums;

/// <summary>
/// An enum for a type of step behavior in the debugger.
/// </summary>
public enum StepMode
{
    /// <summary>
    /// Continue execution until the next pre-set breakpoint.
    /// </summary>
    Continue,

    /// <summary>
    /// Step on instruction.
    /// </summary>
    Step,

    /// <summary>
    /// Step, but skip over call or "and link" instructions.
    /// </summary>
    StepOver,

    /// <summary>
    /// Steps out to the return address.
    /// </summary>
    StepOut,

}
