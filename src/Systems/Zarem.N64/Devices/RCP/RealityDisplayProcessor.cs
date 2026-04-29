// Avishai Dernis 2026

using System;

namespace Zarem.N64.Devices.RCP;

/// <summary>
/// A sub-components of the <see cref="RealityCoProcessor"/> responsible for processing the display operations.
/// </summary>
public class RealityDisplayProcessor
{
    /// <summary>
    /// Writes the specified data at the given offset to the RDP registers.
    /// </summary>
    public void WriteRegister(ulong offset, ReadOnlySpan<byte> data)
    {
    }
}
