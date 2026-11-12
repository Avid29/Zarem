// Avishai Dernis 2025

using CommunityToolkit.Mvvm.DependencyInjection;

namespace Zarem.Services;

/// <summary>
/// A static class for quickly grabbing services through the Ioc.
/// </summary>
/// <remarks>
/// This is somewhat a tracker for places where lazy design needs work.
/// </remarks>
public static class Service
{
    /// <summary>
    /// Gets a service.
    /// </summary>
    public static T Get<T>()
        where T : class
        => Ioc.Default.GetRequiredService<T>();
}
