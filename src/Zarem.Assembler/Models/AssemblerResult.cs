// Avishai Dernis 2025

using System.Collections.Generic;
using Zarem.Assembler.Logging;
using Zarem.Models;
using Zarem.Models.Tables;

namespace Zarem.Assembler.Models;

/// <summary>
/// A class containing the results of an assembler run.
/// </summary>
public class AssemblerResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblerResult"/> class.
    /// </summary>
    public AssemblerResult(bool failed, IReadOnlyList<LogEntry> logs, IReadOnlyList<Symbol> symbols, Module? module = null)
    {
        Failed = failed;
        Logs = logs;
        Symbols = symbols;
        Module = module;
    }

    /// <summary>
    /// Gets whether or not the assembly failed.
    /// </summary>
    public bool Failed { get; }

    /// <summary>
    /// Gets the assmembly log.
    /// </summary>
    public IReadOnlyList<LogEntry> Logs { get; }

    /// <summary>
    /// Gets the list of symbols
    /// </summary>
    public IReadOnlyList<Symbol> Symbols { get;  }

    /// <summary>
    /// Gets the abstract <see cref="Zarem.Models.Module"/> result.
    /// </summary>
    public Module? Module { get; }
}
