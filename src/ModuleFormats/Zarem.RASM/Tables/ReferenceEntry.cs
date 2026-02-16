// Adam Dernis 2024

using System.IO;
using System.Runtime.InteropServices;
using Zarem.Assembler.Models.Tables;
using Zarem.Extensions.System.IO;
using Zarem.Helpers;
using Zarem.Models.Addressing;
using Zarem.RASM.Tables.Enums;
using CommonEntry = Zarem.Models.Tables.ReferenceEntry;
using CommonType = Zarem.Models.Tables.Enums.MipsReferenceType;

namespace Zarem.RASM.Tables;

/// <summary>
/// An entry in the RASM load module's reference table.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct ReferenceEntry : IReferenceEntry<ReferenceEntry>, IBigEndianReadWritable<ReferenceEntry>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RelocationEntry"/> struct.
    /// </summary>
    public ReferenceEntry(uint address, Section section, ReferenceType type)
    {
        Address = address;
        Section = section;
        Type = type;
    }

    /// <summary>
    /// Gets or sets the location of reference, within its section.
    /// </summary>
    [field: FieldOffset(0)]
    public uint Address { get; set; }

    /// <summary>
    /// Gets or sets the index of the symbol name in the string table.
    /// </summary>
    [field: FieldOffset(4)]
    public uint SymbolIndex { get; set; }

    /// <summary>
    /// Gets or sets the section the <see cref="Address"/> belongs in.
    /// </summary>
    [field: FieldOffset(8)]
    public Section Section { get; set; }

    /// <summary>
    /// Gets or sets a <see cref="ReferenceType"/> describing how to preform the reference.
    /// </summary>
    [field: FieldOffset(9)]
    public ReferenceType Type { get; set; }

    /// <summary>
    /// Gets or sets the index of the module containing the reference.
    /// </summary>
    [field: FieldOffset(10)]
    public ushort ModuleIndex { get; set; }

    /// <inheritdoc/>
    public readonly ReferenceEntry Read(Stream stream)
    {
        stream.TryRead<uint>(out var address);
        stream.TryRead<uint>(out var symbolIndex);
        stream.TryRead<byte>(out var section);
        stream.TryRead<byte>(out var type);
        stream.TryRead<ushort>(out var moduleIndex);

        return new()
        {
            Address = address,
            SymbolIndex = symbolIndex,
            Section = (Section)section,
            Type = (ReferenceType)type,
            ModuleIndex = moduleIndex,
        };
    }
    
    /// <inheritdoc/>
    public readonly void Write(Stream stream)
    {
        stream.TryWrite(Address);
        stream.TryWrite(SymbolIndex);
        stream.TryWrite((byte)Section);
        stream.TryWrite((byte)Type);
        stream.TryWrite(ModuleIndex);
    }
    
    /// <inheritdoc/>
    public static ReferenceEntry Convert(CommonEntry entry)
    {
        var section = entry.Location.Section switch
        {
            ".text" => Section.Text,
            ".data" => Section.Data,
            _ => Section.None,
        };

        // Get reference type
        var type = entry.Type switch
        {
            CommonType.Relative32 => ReferenceType.AddFullWord,
            CommonType.Absolute32 => ReferenceType.ReplaceFullWord,
            CommonType.PCRelative16 => ReferenceType.AddSimpleImmediate,
            CommonType.Low16 => ReferenceType.ReplaceSimpleImmediate,
            CommonType.JumpTarget26 => ReferenceType.ReplaceAddress,
            _ => ReferenceType.SimpleImmediate,
        };

        // Construct new entry
        return new ReferenceEntry((uint)entry.Location.Value, section, type) ;
    }
    
    /// <inheritdoc/>
    public readonly CommonEntry Convert(string name)
    {
        var section = Section switch
        {
            Section.Text => ".text",
            Section.Data => ".data",
            _ => null,
        };

        var adr = new Address(Address, section);

        var type = Type switch
        {
            ReferenceType.AddFullWord => CommonType.Relative32,
            ReferenceType.ReplaceFullWord => CommonType.Absolute32,
            ReferenceType.AddSimpleImmediate => CommonType.PCRelative16,
            ReferenceType.ReplaceSimpleImmediate => CommonType.Low16,
            ReferenceType.ReplaceAddress => CommonType.JumpTarget26,
            _ => CommonType.Low16,
        };

        return new CommonEntry(name, adr, type);
    }
}
