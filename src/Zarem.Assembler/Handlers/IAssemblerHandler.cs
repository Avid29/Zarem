// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Assembler.Models;
using Zarem.Assembler.Tokenization.Interfaces;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Models;
using Zarem.Models.Tables;

namespace Zarem.Assembler.Handlers;

/// <summary>
/// An interface for an architecture specific assembler behavior handler.
/// </summary>
public interface IAssemblerHandler
{
    /// <summary>
    /// Gets the name of the architecture.
    /// </summary>
    string GetArchitectureName();

    /// <summary>
    /// Gets the tokenizer profile for the architecture.
    /// </summary>
    ITokenizerProfile TokenizerProfile { get; }

    /// <summary>
    /// Gets the size of an instruction.
    /// </summary>
    /// <param name="line">The line of assembly.</param>
    /// <returns>The size of the instruction in bytes.</returns>
    int GetInstructionSize(AssemblyLine line);

    /// <summary>
    /// Parse an instruction for the architecture.
    /// </summary>
    IParsedInstruction? ParseInstruction(AssemblyLine line, Address address, IReadOnlyDictionary<string, Symbol> symbols, ILogger? logger);

    /// <summary>
    /// Gets a nop instruction for the architecture.
    /// </summary>
    ReadOnlySpan<byte> GetNOP();
}
