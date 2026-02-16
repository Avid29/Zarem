// Adam Dernis 2024

using System.IO;
using System.Runtime.InteropServices;
using Zarem.Assembler.Models.Tables;
using Zarem.Extensions.System.IO;
using Zarem.Helpers;
using Zarem.Models.Tables;
using Zarem.RASM.Tables.Enums;
using CommonEntry = Zarem.Models.Tables.Symbol;

namespace Zarem.RASM.Tables;

/// <summary>
/// An entry in the RASM load module's symbol table.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct SymbolEntry : ISymbolEntry<SymbolEntry>, IBigEndianReadWritable<SymbolEntry>
{
    private const byte SectionBitCount = 4;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SymbolEntry"/> struct.
    /// </summary>
    public SymbolEntry(uint address, Section section)
    {
        Value = address;
        Section = section;
    }

    /// <summary>
    /// Gets or sets flags on the symbol entry.
    /// </summary>
    [field: FieldOffset(0)]
    public SymbolFlags Flags { get; set; }

    /// <summary>
    /// Gets or sets the symbol's value.
    /// </summary>
    [field: FieldOffset(4)]
    public uint Value { get; set; }

    /// <summary>
    /// Gets or sets the index of the symbol name in the string table.
    /// </summary>
    [field: FieldOffset(8)]
    public uint SymbolIndex { get; set; }

    /// <summary>
    /// Gets or sets the index of the object file in the object file table.
    /// </summary>
    [field: FieldOffset(12)]
    public ushort ObjectFileIndex { get; set; }

    /// <summary>
    /// Fills the struct to 16 bytes.
    /// </summary>
    [field: FieldOffset(14)]
    private readonly ushort _filler;

    /// <summary>
    /// Gets or sets the section specified by the 
    /// </summary>
    public Section Section
    {
        readonly get => (Section)UintMasking.GetShiftMask((uint)Flags, SectionBitCount, 0);
        set
        {
            var cast = (uint)Flags;
            UintMasking.SetShiftMask(ref cast, SectionBitCount, 0, (uint)value);
            Flags = (SymbolFlags)cast;
        }
    }

    /// <summary>
    /// Gets if the symbol is defined.
    /// </summary>
    public readonly bool IsDefined => Flags.HasFlag(SymbolFlags.Defined);

    /// <summary>
    /// Gets if a flag is set on the symbol entry.
    /// </summary>
    /// <param name="flag">The flag to check.</param>
    /// <returns>True if the flag is set, false otherwise.</returns>
    public readonly bool CheckFlag(SymbolFlags flag) => Flags.HasFlag(flag);

    /// <summary>
    /// Sets a set of flags on the symbol entry.
    /// </summary>
    /// <param name="flags">The flags to set.</param>
    /// <param name="state">The new state of the flag.</param>
    public void SetFlags(SymbolFlags flags, bool state)
    {
        // Clear the flag.
        Flags &= ~flags;

        // Set the flag to true.
        if (state)
        {
            Flags |= flags;
        }
    }

    /// <inheritdoc/>
    public readonly SymbolEntry Read(Stream stream)
    {
        stream.TryRead<uint>(out var flags);
        stream.TryRead<uint>(out var value);
        stream.TryRead<uint>(out var symbolIndex);
        stream.TryRead<ushort>(out var objFileIndex);
        stream.TryRead<byte>(out var section);
        stream.TryRead<byte>(out _);

        return new()
        {
            Flags = (SymbolFlags)flags,
            Value = value,
            SymbolIndex = symbolIndex,
            ObjectFileIndex = objFileIndex,
        };
    }
    
    /// <inheritdoc/>
    public readonly void Write(Stream stream)
    {
        stream.TryWrite((uint)Flags);
        stream.TryWrite(Value);
        stream.TryWrite(SymbolIndex);
        stream.TryWrite(ObjectFileIndex);
        stream.TryWrite(_filler);
    }
    
    /// <inheritdoc/>
    public static SymbolEntry Convert(CommonEntry entry)
    {
        //// Initialize symbol
        //var symbol = new SymbolEntry(entry.Address);

        //// Set flags
        //symbol.SetFlags(SymbolFlags.Global, entry.Binding.HasFlag(SymbolBinding.Global));
        //symbol.SetFlags(SymbolFlags.Def_Label, entry.Type is SymbolType.Label);

        //// Return
        //return symbol;

        return default;
    }
    
    /// <inheritdoc/>
    /// <remarks>
    /// <see cref="CommonEntry.Name"/> will be null.
    /// </remarks>
    public readonly CommonEntry Convert(string name)
    {
        //// Get type
        //var type = SymbolType.Macro;
        //if (CheckFlag(SymbolFlags.Def_Label))
        //    type = SymbolType.Label;

        //// Get address and initialize
        //var adr = new Address(Value, Section);
        //var symbol = new CommonEntry(name, type, adr) 
        //{
        //    // Set flags
        //    Binding = CheckFlag(SymbolFlags.Global) ? SymbolBinding.Global : SymbolBinding.Local,
        //}; 

        //return symbol;

        return new CommonEntry(name, null);
    }
}
