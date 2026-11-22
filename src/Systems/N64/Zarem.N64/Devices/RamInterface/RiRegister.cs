// Avishai Dernis 2026

namespace Zarem.N64.Devices.RamInterface;

/// <summary>
/// An enum for the fixed registers in the <see cref="N64RamInterface"/>.
/// </summary>
public enum RiRegister
{
    /// <summary>
    /// RDRAM mode configuration register.
    /// </summary>
    /// <remarks>
    /// RI_MODE_REG
    /// </remarks>
    Mode = 0x00,

    /// <summary>
    /// RDRAM control/config register.
    /// </summary>
    /// <remarks>
    /// RI_CONFIG_REG
    /// </remarks>
    Config = 0x04,

    /// <summary>
    /// RDRAM current load register.
    /// </summary>
    /// <remarks>
    /// RI_CURRENT_LOAD_REG
    /// </remarks>
    CurrentLoad = 0x08,

    /// <summary>
    /// RDRAM device select register.
    /// </summary>
    /// <remarks>
    /// RI_SELECT_REG
    /// </remarks>
    Select = 0x0C,

    /// <summary>
    /// RDRAM refresh register.
    /// </summary>
    /// <remarks>
    /// RI_REFRESH_REG
    /// </remarks>
    Refresh = 0x10,

    /// <summary>
    /// RDRAM latency register.
    /// </summary>
    /// <remarks>
    /// RI_LATENCY_REG
    /// </remarks>
    Latency = 0x14,

    /// <summary>
    /// RDRAM error register.
    /// </summary>
    /// <remarks>
    /// RI_ERROR_REG
    /// </remarks>
    Error = 0x18,

    /// <summary>
    /// RDRAM error log register.
    /// </summary>
    /// <remarks>
    /// RI_WERROR_REG
    /// </remarks>
    ErrorLog = 0x1C,
}
