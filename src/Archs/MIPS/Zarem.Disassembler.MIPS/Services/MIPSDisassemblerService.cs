// Avishai Dernis 2025

#if DEBUG

using Zarem.Assembler;
using Zarem.Models.Instructions;
using Zarem.Services.Interfaces;

namespace Zarem.Disassembler.Services;

/// <summary>
/// An implementation of the <see cref="IDisassemblerService"/>.
/// </summary>
public class MipsDisassemblerService : IDisassemblerService
{
    private MipsDisassembler _disassembler;

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
