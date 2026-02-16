// Adam Dernis 2024

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Zarem.Assembler.Models;
using Zarem.Emulator.Models.Modules;
using Zarem.RASM;
using Zarem.RASM.Config;

namespace ObjFormats.RASM;

/// <summary>
/// A fully assembled object module in RASM format.
/// </summary>
public partial class RasmModule : IBuildModule<RasmModule, RasmConfig>, IExecutableModule
{
    private static readonly string[] SectionNames =
    {
        ".text",
        ".rodata",
        ".data",
        ".sdata",
        ".sbss",
        ".bss",
    };

    private readonly Stream _source;

    /// <summary>
    /// Initializes a new instance of the <see cref="RasmModule"/> class.
    /// </summary>
    public RasmModule(string? name, Header header, Stream source)
    {
        Name = name;
        Header = header;
        _source = source;
    }

    /// <summary>
    /// Gets the module header info.
    /// </summary>
    public Header Header { get; private set; }

    /// <inheritdoc/>
    public uint EntryAddress => Header.EntryPoint;
    
    /// <inheritdoc/>
    public string? Name { get; }

    /// <inheritdoc/>
    public void Load(Stream destination)
    {
        throw new NotImplementedException();
    }

    private void ResetStream(bool skipHeader = true)
    {
        _source.Position = skipHeader ? Header.HEADER_SIZE : 0;
    }

    private bool ValidateHeader(RasmConfig config)
    {
        // Ensure the magic number is correct
        if (Header.Magic != config.MagicNumber)
            return false;

        // Ensure the version number is correct
        if (Header.Version != config.VersionNumber)
            return false;

        // Ensure the module is the expected size
        if (Header.ExpectedModuleSize != _source.Length)
            return false;

        return true;
    }

    private Dictionary<int, string> LoadStrings()
    {
        var strings = new Dictionary<int, string>();
        var sb = new StringBuilder();
        int pos = 0;
        int c;

        while ((c = _source.ReadByte()) is not -1)
        {
            if (c is not 0)
            {
                sb.Append((char)c);
                continue;
            }

            var str = $"{sb}";
            strings.Add(pos, str);
            sb = new StringBuilder();
            pos += str.Length + 1;
        }
        return strings;
    }

    /// <inheritdoc/>
    public async Task SaveAsync(Stream stream)
    {
        _source.Position = 0;
        await _source.CopyToAsync(stream);
    }
}
