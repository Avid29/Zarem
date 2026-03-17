// Avishai Dernis 2026

using System;
using System.Collections.Generic;

namespace Zarem.Debugger.Viewer;

/// <summary>
/// An interface for describing a group of registers in a CPU.
/// </summary>
public interface IRegisterGroup
{
    /// <summary>
    /// An event invoked when a register is updated.
    /// </summary>
    event EventHandler<IRegisterGroup, string>? RegisterUpdated;

    /// <summary>
    /// Gets the names of the CPU registers.
    /// </summary>
    IEnumerable<string> RegisterNames { get; }

    /// <summary>
    /// Gets or sets the value of a CPU register.
    /// </summary>
    /// <param name="registerName"></param>
    /// <returns></returns>
    ulong? this[string registerName] { get; set; }
}
