// Avishai Dernis 2025

namespace Zarem.Models.Enums;

/// <summary>
/// An enum indicating the ide state.
/// </summary>
public enum IdeState
{
    #pragma warning disable CS1591
    
    NotReady,
    Ready,
    Building,
    BuildComplete,
    Runnning,
    Failed,
    
    # pragma warning restore CS1591
}
