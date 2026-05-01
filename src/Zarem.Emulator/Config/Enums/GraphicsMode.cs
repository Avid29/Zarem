// Avishai Dernis 2026

using System.Xml.Serialization;

namespace Zarem.Emulator.Config.Enums;

/// <summary>
/// An enum for the graphics execution mode.
/// </summary>
public enum GraphicsMode
{
#pragma warning disable CS1591
    [XmlEnum("auto")] Auto = 0,
    [XmlEnum("software")] Software = 1,
    [XmlEnum("dx11")] DirectX11 = 4,
    [XmlEnum("dx12")] DirectX12 = 5,
    [XmlEnum("opengl")] OpenGL = 2,
    [XmlEnum("vulkan")] Vulkan = 3,
#pragma warning restore CS1591
}
