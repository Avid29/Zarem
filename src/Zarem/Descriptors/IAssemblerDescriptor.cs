// Avishai Dernis 2026

using System;
using Zarem.Descriptors.Base;

namespace Zarem.Descriptors;

/// <summary>
/// An interface for a class describing an assembler.
/// </summary>
public interface IAssemblerDescriptor : IConfigDescriptor
{
    /// <summary>
    /// Gets the <see cref="Type"/> of the assembler's architecture handler.
    /// </summary>
    Type AssemblerHandlerType { get; }
}
