// Adam Dernis 2024

using System;
using System.Collections.Generic;
using System.Linq;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Localization;

namespace Zarem.Assembler.Logging;

/// <summary>
/// An <see cref="ILogger"/> for assembly/linker errors, warnings, and messages.
/// </summary>
public class Logger : ILogger
{
    private readonly HashSet<string> _localizers = [];
    private readonly CompositeLocalizer _localizer;
    private readonly List<LogEntry> _currentLogs;
    private readonly List<LogEntry> _flushedLogs;

    private bool _currentFailed;

    /// <summary>
    /// An event invoked when an <see cref="LogEntry"/> is logged.
    /// </summary>
    public event EventHandler<LogEntry>? EntryLogged;

    /// <summary>
    /// Initializes a new instance of the <see cref="Logger"/> class.
    /// </summary>
    public Logger()
    {
        // TODO: Architecture specific logging IDs/Codes
        _localizer = new CompositeLocalizer();
        _currentLogs = [];
        _flushedLogs = [];
    }

    /// <summary>
    /// Gets a value indicating whether or not assembly failed.
    /// </summary>
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
    public void Register(IStringLocalizer localizer)
    {
        if (localizer.Namespace is null || _localizers.Contains(localizer.Namespace))
            return;

        _localizers.Add(localizer.Namespace);
        _localizer.Register(localizer);
    }

    /// <inheritdoc/>
    public bool Log(Severity severity, LogCode code, string? file, string messageKey, params object?[] args)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public bool Log(Severity severity, LogCode code, Token token, string messageKey, params object?[] args)
        => Log(severity, code, [token], messageKey, args);
    
    /// <inheritdoc/>
    public bool Log(Severity severity, LogCode code, ReadOnlySpan<Token> tokens, string messageKey, params object?[] args)
    {
        var formattedMessage = _localizer[messageKey, args];
        formattedMessage ??= messageKey;

        var log = new LogEntry(severity, code, formattedMessage, tokens.ToArray());
        _currentLogs.Add(log);
        EntryLogged?.Invoke(this, log);

        if (severity is Severity.Error)
            CurrentFailed = true;

        return severity is not Severity.Error;
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

    /// <summary>
    /// Gets a readonly list of logs for the current file.
    /// </summary>
    public IReadOnlyList<ILog> CurrentLog => _currentLogs;

    /// <summary>
    /// Gets a readonly list of logs.
    /// </summary>
    public IEnumerable<ILog> Logs => _currentLogs.Concat(_flushedLogs);
}
