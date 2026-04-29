// Avishai Dernis 2026

using System;

namespace Zarem.N64.Devices.RCP;

/// <summary>
/// A sub-components of the <see cref="RealityCoProcessor"/> responsible for output and timing.
/// </summary>
public class N64VideoInterface
{
    /// <summary>
    /// Writes the specified data at the given offset to the video interface registers.
    /// </summary>
    public void WriteRegister(ulong offset, ReadOnlySpan<byte> data)
    {
    }
}
