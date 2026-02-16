// Avishai Dernis 2025

using System.Collections.Generic;
using System.IO;
using System.Text;
using Zarem.Models;
using Zarem.Models.Tables;
using Zarem.RASM;
using Zarem.RASM.Config;
using RasmReference = Zarem.RASM.Tables.ReferenceEntry;
using RasmRelocation = Zarem.RASM.Tables.RelocationEntry;
using RasmSymbol = Zarem.RASM.Tables.SymbolEntry;

namespace ObjFormats.RASM;

public partial class RasmModule
{
    private struct RasmBuildContext
    {
        private readonly Stream _stringStream;
        private Dictionary<string, long> _stringDict;

        public RasmBuildContext(Module source, RasmConfig rasmConfig)
        {
            Source = source;
            Config = rasmConfig;
            Stream = new MemoryStream();

            _stringDict = [];
            _stringStream = new MemoryStream();
        }

        public Header Header { get; private set;  }

        public Module Source { get; }

        public RasmConfig Config { get; }

        public Stream Stream { get; }

        public IReadOnlyDictionary<string, long> StringEntries => _stringDict;

        public void InitAndAllocHeader()
        {
            Source.Sections.TryGetValue(".text", out Section? text);
            Source.Sections.TryGetValue(".rodata", out Section? rodata);
            Source.Sections.TryGetValue(".data", out Section? data);
            Source.Sections.TryGetValue(".sdata", out Section? sdata);
            Source.Sections.TryGetValue(".sbss", out Section? sbss);
            Source.Sections.TryGetValue(".bss", out Section? bss);

            var sizes = new uint[]
            {
                (uint)(text?.Stream.Length ?? 0),       // text
                (uint)(rodata?.Stream.Length ?? 0),     // rodata
                (uint)(data?.Stream.Length ?? 0),       // data
                (uint)(sdata?.Stream.Length ?? 0),      // sdata
                (uint)(sbss?.Stream.Length ?? 0),       // sbss
                (uint)(bss?.Stream.Length ?? 0),        // bss
                0,                                      // rel count
                0,                                      // ref count
                (uint)Source.Symbols.Count,             // symbol count
                0,
            };

            Header = new Header(Config.MagicNumber, Config.VersionNumber, 0, 0, sizes);
            Header.TryWrite(Stream);
        }

        public void AppendSimpleSegments()
        {
            Source.ResetStreamPositions();
            foreach (var section in Source.Sections.Values)
                section.Stream.CopyTo(Stream);
        }

        public void ConvertReferences()
        {
            uint relCount = 0;
            uint refCount = 0;
            Stream relStream = new MemoryStream();
            Stream refStream = new MemoryStream();

            // Write reference and relocation table to their streams
            foreach (var reference in Source.References)
            {
                if (reference.Symbol is null)
                {
                    // Convert to rasm relocation and write to relocation stream
                    var newRelocation = RasmRelocation.Convert(reference);
                    newRelocation.Write(relStream);
                    relCount++;
                }
                else
                {
                    // Convert to rasm relocation, extract string, and write to reference stream
                    var newReference = RasmReference.Convert(reference);
                    newReference.SymbolIndex = (uint)GetOrAddString(reference.Symbol);
                    newReference.Write(refStream);
                    refCount++;
                }
            }

            // Append rel and ref streams
            relStream.Position = 0;
            refStream.Position = 0;
            relStream.CopyTo(Stream);
            refStream.CopyTo(Stream);
            var header = Header;
            header.RelocationTableCount = relCount;
            header.ReferenceTableCount = refCount;
            Header = header;
        }

        public void ConvertSymbols()
        {
            foreach (var symbol in Source.Symbols.Values)
            {
                // Convert to rasm, extract string, and write to stream
                var newSymbol = RasmSymbol.Convert(symbol);
                newSymbol.SymbolIndex = (uint)GetOrAddString(symbol.Name);
                newSymbol.Write(Stream);
            }
        }

        public void FlushStrings()
        {
            _stringStream.Position = 0;
            _stringStream.CopyTo(Stream);

            var header = Header;
            header.StringTableSize = (uint)_stringStream.Length;
            Header = header;
        }

        public void FinalizeModule()
        {
            // Mark the end of the module
            Stream.SetLength(Stream.Position);

            // Rewrite the header
            Stream.Position = 0;
            Header.TryWrite(Stream);
        }

        private long GetOrAddString(string symbol)
        {
            // Get from table if already seen
            if (_stringDict.TryGetValue(symbol, out long value))
                return value;

            // Append to table
            var pos = _stringStream.Position;
            _stringStream.Write(Encoding.UTF8.GetBytes(symbol));
            _stringStream.WriteByte(0); // Null terminate

            // Log in table and return position
            _stringDict.Add(symbol, pos);
            return pos;
        }
    }

    /// <inheritdoc/>
    public static RasmModule? Create(Module constructor, RasmConfig config)
    {
        // TODO: Flags and entry point properly
        // TODO: Construct string list.

        // Create context
        var context = new RasmBuildContext(constructor, config);

        // Allocate space for header
        context.InitAndAllocHeader();

        // Append segments to stream
        context.AppendSimpleSegments();

        // Create split streams for relocations and references
        context.ConvertReferences();

        // Write symbol table to the stream
        context.ConvertSymbols();

        // Write strings to the stream
        context.FlushStrings();

        // Finalize
        context.FinalizeModule();

        // Reset position, flush, and load
        context.Stream.Position = 0;
        context.Stream.Flush();
        return Open(constructor.Name, context.Stream);
    }
}
