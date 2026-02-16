// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Models;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Models;
using Zarem.Models.Tables;

namespace Zarem.Assembler.Handlers;

/// <summary>
/// An interface for an handling architecture specific assembler functions.
/// </summary>
public interface IAssemblerHandler
{
    /// <summary>
    /// Gets the name of the architecture.
    /// </summary>
    string GetArchitectureName();

    /// <summary>
    /// Gets the size of an instruction.
    /// </summary>
    /// <param name="line">The line of assembly.</param>
    /// <returns>The size of the instruction in bytes.</returns>
    int GetInstructionSize(AssemblyLine line);

    /// <summary>
    /// Parse an instruction for the architecture.
    /// </summary>
    IParsedInstruction? ParseInstruction(AssemblyLine line, Address address, IReadOnlyDictionary<string, Symbol> symbols, Logger logger);

    /// <summary>
    /// Gets a nop instruction for the architecture.
    /// </summary>
    ReadOnlySpan<byte> GetNOP();
}
