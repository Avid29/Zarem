// Avishai Dernis 2026

using System.Collections.Generic;
using Zarem.Debugger.Models;

namespace Zarem.Debugger.Viewer;

/// <summary>
/// An interface for describing a group of registers in a CPU.
/// </summary>
public interface IRegisterViewer
{
    /// <summary>
    /// Gets the names of the CPU registers.
    /// </summary>
    IEnumerable<RegisterMeta> Registers { get; }

    /// <summary>
    /// Gets or sets the value of a CPU register.
    /// </summary>
    /// <param name="registerName"></param>
    /// <returns></returns>
    ulong? this[string registerName] { get; set; }
}
