// Avishai Dernis 2025

using LibObjectFile.Elf;
using System.IO;
using System.Threading.Tasks;
using Zarem.Assembler.Models;
using Zarem.Elf.Config;
using Zarem.Emulator.Models.Modules;
using Zarem.Models.Abstract;

namespace Zarem.Elf;

/// <summary>
/// An object module in ELF format.
/// </summary>
public partial class ElfModule : ModuleBase, IBuildModule<ElfModule, ElfConfig>, IExecutableModule
{
    private readonly ElfFile _elfFile;

    private ElfModule(string? name,  ElfFile elfFile) : base(name)
    {
        _elfFile = elfFile;
    }

    /// <inheritdoc/>
    public uint EntryAddress => (uint)_elfFile.EntryPointAddress;

    /// <inheritdoc/>
    public static ElfModule? Open(string? name, Stream stream)
    {
        var elfFile = ElfFile.Read(stream);
        return new ElfModule(name, elfFile);
    }

    /// <inheritdoc/>
    public async Task SaveAsync(Stream stream)
    {
        var diagnostics = _elfFile.Verify();
        _elfFile.Write(stream);
    }
}
