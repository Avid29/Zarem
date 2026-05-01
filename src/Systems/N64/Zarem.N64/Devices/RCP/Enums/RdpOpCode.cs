// Avishai Dernis 2026

namespace Zarem.N64.Devices.RCP.Enums;

/// <summary>
/// An enum representing the various RDP command opcodes. These opcodes are used to identify the type of command being issued to the RDP and determine how to process it.
/// </summary>
public enum RdpOpCode : byte
{
#pragma warning disable CS1591

    Tri_Z = 0x1,
    Tri_T = 0x2,
    Tri_S = 0x4,

    Triangle = 0x08,
    TriangleZ = Triangle | Tri_Z,
    TriangleT = Triangle | Tri_T,
    TriangleTZ = Triangle | Tri_T | Tri_Z,
    TriangleS = Triangle | Tri_S,
    TriangleSZ = Triangle | Tri_S | Tri_Z,
    TriangleST = Triangle | Tri_S | Tri_T,
    TriangleSTZ = Triangle | Tri_S | Tri_T | Tri_Z,

    TextureRectangle = 0x24,
    TextureRectangleFlip = 0x25,
    FillRectangle = 0x30,

    PipeSync = 0x27,
    TileSync = 0x28,
    FullSync = 0x29,
    LoadSync = 0x26,

    SetKeyGB = 0x2A,
    SetKeyR = 0x2B,
    SetConvert = 0x2C,
    SetScissor = 0x2D,
    SetPrimDepth = 0x2E,
    SetOtherModes = 0x2F,
    SetTileSize = 0x32,
    LoadBlock = 0x33,
    LoadTile = 0x34,
    SetTile = 0x35,
    FillColor = 0x36,
    FogColor = 0x37,
    BlendColor = 0x38,
    PrimColor = 0x39,
    EnvColor = 0x3A,
    Combine = 0x3C,
    SetTextureImage = 0x3D,
    SetMaskImage = 0x3E,
    SetColorImage = 0x3F
#pragma warning restore CS1591
}
