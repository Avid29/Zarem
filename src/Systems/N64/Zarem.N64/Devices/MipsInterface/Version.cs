// Avishai Dernis 2026

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Zarem.N64.Devices.MipsInterface;

/// <summary>
/// A struct for parsing the <see cref="MiRegister.Version"/> register.
/// </summary>
/// <remarks>
/// https://n64brew.dev/wiki/MIPS_Interface#0x0430_0004_-_MI_VERSION
/// </remarks>
[StructLayout(LayoutKind.Explicit)]
public struct Version
{
    [FieldOffset(0)] private byte _ioVersion;
    [FieldOffset(4)] private byte _racVersion;
    [FieldOffset(8)] private byte _rdpVersion;
    [FieldOffset(12)] private byte _rspVersion;

    /// <summary>
    /// Gets or sets the IO hardware version.
    /// </summary>
    public byte IOVersion
    {
        readonly get => _ioVersion;
        set => _ioVersion = value;
    }

    /// <summary>
    /// Gets or sets the RAC hardware version.
    /// </summary>
    public byte RacVersion
    {
        readonly get => _racVersion;
        set => _racVersion = value;
    }

    /// <summary>
    /// Gets or sets the RDP hardware version.
    /// </summary>
    public byte RdpVersion
    {
        readonly get => _rdpVersion;
        set => _rdpVersion = value;
    }

    /// <summary>
    /// Gets or sets the RSP hardware version.
    /// </summary>
    public byte RspVersion
    {
        readonly get => _rspVersion;
        set => _rspVersion = value;
    }

    /// <summary>
    /// Casts a <see cref="uint"/> to a <see cref="Version"/>.
    /// </summary>
    public static explicit operator Version(uint value) => Unsafe.BitCast<uint, Version>(value);

    /// <summary>
    /// Casts a <see cref="Version"/> to a <see cref="uint"/>.
    /// </summary>
    public static explicit operator uint(Version value) => Unsafe.BitCast<Version, uint>(value);
}
