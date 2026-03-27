// Avishai Dernis 2026

using System;
using Zarem.Localization;

namespace Zarem.Emulator.Exceptions;

/// <summary>
/// A base class for an emulation exception.
/// </summary>
public abstract class EmulationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmulationException"/> class
    /// </summary>
    public EmulationException(string? message = null, ulong? address = null)
    {
        Message = message ?? string.Empty;
        Address = address;
    }

    internal EmulationException(ulong? address, string messageKey, params object[] args)
    {
        var localizer = new Localizer("Zarem.Emulator.Resources.Messages", typeof(Zaremulator).Assembly);
        Message = localizer[messageKey, args] ?? string.Empty;
        Address = address;
    }

    /// <summary>
    /// Gets the exception message.
    /// </summary>
    public override string Message { get; }

    /// <summary>
    /// Gets the program counter address where the exception occured.
    /// </summary>
    public ulong? Address { get; }
}
