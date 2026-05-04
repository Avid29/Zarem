// Avishai Dernis 2025

#if DEBUG

using Zarem.Mips.Assembler;
using Zarem.Mips.Models.Instructions;
using Zarem.Mips.Services.Interfaces;

namespace Zarem.Mips.Disassembler.Services;

/// <summary>
/// An implementation of the <see cref="IDisassemblerService"/>.
/// </summary>
public class MipsDisassemblerService : IDisassemblerService
{
    private readonly MipsDisassembler _disassembler;

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsDisassemblerService"/> class.
    /// </summary>
    public MipsDisassemblerService(MipsAssemblerConfig config)
    {
        _disassembler = new MipsDisassembler(config);
    }

    /// <inheritdoc/>
    public string Disassemble(MipsInstruction instruction)
        => _disassembler.Disassemble(instruction);
}

#endif
