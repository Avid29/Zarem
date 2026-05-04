// Avishai Dernis 2026

namespace Zarem.Mips.Disassembler.Models;

/// <summary>
/// A type describing the info to lookup an instruction in a disassembler instruction table.
/// </summary>
public record struct DisassemblerLookup(byte OpCode, byte FuncCode = 0, byte FuncCode2 = 255, bool IsFloat = false);
