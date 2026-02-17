// Adam Dernis 2024

using System;
using System.Collections.Generic;
using System.Linq;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;

namespace Zarem.Assembler.Logging;

/// <summary>
/// An <see cref="ILogger"/> implementation for assembly/linker errors, warnings, and messages.
/// </summary>
public class Logger : ILogger
{
    private readonly List<ILog> _currentLogs;
    private readonly List<ILog> _flushedLogs;

    private bool _currentFailed;

    /// <inheritdoc/>
    public event EventHandler<ILog>? EntryLogged;

    /// <summary>
    /// Initializes a new instance of the <see cref="Logger"/> class.
    /// </summary>
    public Logger()
    {
        _currentLogs = [];
        _flushedLogs = [];
    }

    /// <inheritdoc/>
    public bool CurrentFailed
    {
        get => _currentFailed;
        set
        {
            _currentFailed = value;
            if (value)
            {
                Failed = true;
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether or not assembly failed.
    /// </summary>
    public bool Failed { get; private set; }

    /// <inheritdoc/>
    public bool Log(ILog log)
    {
        _currentLogs.Add(log);
        EntryLogged?.Invoke(this, log);

        if (log.Severity is Severity.Error)
            CurrentFailed = true;

        return log.Severity is not Severity.Error;
    }

    /// <inheritdoc/>
    public void Flush()
    {
        if (_currentLogs.Count is 0)
            return;

        CurrentFailed = false;
        _flushedLogs.AddRange(_currentLogs);
        _currentLogs.Clear();
    }

    /// <inheritdoc/>
    public IReadOnlyList<ILog> CurrentLog => _currentLogs;

    /// <inheritdoc/>
    public IEnumerable<ILog> Logs => _currentLogs.Concat(_flushedLogs);
}
