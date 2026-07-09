// Avishai Dernis 2026

namespace Zarem.Config;

/// <summary>
/// A base class for all zarem component configuration types.
/// </summary>
public abstract class ConfigBase : IConfig
{
    /// <inheritdoc/>
    public virtual object Clone() => MemberwiseClone();
}
