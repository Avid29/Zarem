// Adam Dernis 2024

using System.IO;
using System.Runtime.InteropServices;
using Zarem.Assembler.Models.Tables;
using Zarem.Extensions.System.IO;
using Zarem.Helpers;
using Zarem.RASM.Tables.Enums;
using CommonEntry = Zarem.Models.Tables.ReferenceEntry;

namespace Zarem.RASM.Tables;

/// <summary>
/// An entry in the RASM load module's relocation table.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct RelocationEntry : IReferenceEntry<RelocationEntry>, IBigEndianReadWritable<RelocationEntry>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RelocationEntry"/> struct.
    /// </summary>
    public RelocationEntry(uint address, Section section, RelocationType type)
    {
        Address = address;
        Section = section;
        Type = type;
    }

    /// <summary>
    /// Gets the address to be relocated.
    /// </summary>
    [field: FieldOffset(0)]
    public uint Address { get; set; }

    /// <summary>
    /// Gets the section number.
    /// </summary>
    [field: FieldOffset(4)]
    public Section Section { get; set; }

    /// <summary>
    /// Gets a <see cref="RelocationType"/> describing how to preform the relocation.
    /// </summary>
    [field: FieldOffset(5)]
    public RelocationType Type { get; set; }

    /// <summary>
    /// Fills the struct to 8 bytes.
    /// </summary>
    [field: FieldOffset(6)]
    private readonly ushort _filler;

    /// <inheritdoc/>
    public readonly RelocationEntry Read(Stream stream)
    {
        stream.TryRead<uint>(out var address);
        stream.TryRead<byte>(out var section);
        stream.TryRead<byte>(out var type);
        stream.TryRead<ushort>(out _);

        return new(address, (Section)section, (RelocationType)type);
    }
    
    /// <inheritdoc/>
    public readonly void Write(Stream stream)
    {
        stream.TryWrite(Address);
        stream.TryWrite((byte)Section);
        stream.TryWrite((byte)Type);
        stream.TryWrite(_filler);
    }
    
    /// <inheritdoc/>
    public static RelocationEntry Convert(CommonEntry entry)
    {
        //var type = entry.Type switch
        //{
        //    CommonType.Absolute32 => RelocationType.FullWord,
        //    CommonType.Low16 => RelocationType.SimpleImmediate,
        //    CommonType.JumpTarget26 => RelocationType.Address,
        //    _ => RelocationType.SimpleImmediate,
        //};

        //return new RelocationEntry(entry.Location, type);

        return default;
    }
    
    /// <inheritdoc/>
    public readonly CommonEntry Convert(string? name)
    {
        //if (name is not null)
        //{
        //    ThrowHelper.ThrowArgumentException(nameof(name));
        //}


        //var adr = new Address(Address, Section);

        //var type = Type switch
        //{
        //    RelocationType.FullWord => CommonType.Absolute32,
        //    RelocationType.SimpleImmediate => CommonType.Low16,
        //    RelocationType.Address => CommonType.JumpTarget26,
        //    _ => CommonType.Low16,
        //};

        //return new CommonEntry(null, adr, type, ReferenceMethod.Relocate);

        return default;
    }
}
