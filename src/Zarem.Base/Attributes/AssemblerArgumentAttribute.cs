// Avishai Dernis 2026

using System;

namespace Zarem.Attributes;

/// <summary>
/// A base class for an attribute that describes how to parse an assembler argument.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public abstract class AssemblerArgumentAttribute : Attribute
{
}
