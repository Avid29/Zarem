// Avishai Dernis 2025

namespace Zarem.IDE.Models.Enums;

/// <summary>
/// An enum indicating the ide state.
/// </summary>
public enum IdeState
{
    #pragma warning disable CS1591
    
    NotReady,
    Ready,
    Building,
    BuildCompleted,
    BuildFailed,
    Running,
    Debugging,
    
    # pragma warning restore CS1591
}
