// Avishai Dernis 2026

using System.Numerics;
using System.Runtime.InteropServices;

namespace Zarem.N64.Devices.RCP.Models;

#pragma warning disable CS1591

[StructLayout(LayoutKind.Sequential)]
public struct RdpVertex
{
    public Vector4 Position; // x, y, z, w
    public Vector2 TexCoord; // s, t
}
