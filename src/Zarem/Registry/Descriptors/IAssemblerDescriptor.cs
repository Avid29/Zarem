// Avishai Dernis 2026

using System;
using Zarem.Descriptors;

namespace Zarem.Registry.Descriptors;

/// <summary>
/// An interface for a class describing an assembler.
/// </summary>
public interface IAssemblerDescriptor : IDescriptor
{
    /// <summary>
    /// Gets the <see cref="Type"/> of the assembler's architecture handler.
    /// </summary>
    Type AssemblerHandlerType { get; }
}
