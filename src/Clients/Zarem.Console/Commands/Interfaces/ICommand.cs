// Avishai Dernis 2026

namespace Zarem.Console.Commands.Interfaces;

/// <summary>
/// An interface for a console command.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Gets the name key of the command.
    /// </summary>
    static abstract string NameKey { get; }
}
