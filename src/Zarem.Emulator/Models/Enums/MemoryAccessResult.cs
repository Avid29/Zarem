// Avishai Dernis 2026

namespace Zarem.Emulator.Models.Enums;

/// <summary>
/// An enum describing the result of a memory access attempt.
/// </summary>
public enum MemoryAccessResult
{
    /// <summary>
    /// The memory operation was completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The address is invalid, misaligned, or outside the addressable space.
    /// </summary>
    AddressError,

    /// <summary>
    /// The address could not be resolved by the translation mechanism (e.g., page fault / TLB miss).
    /// </summary>
    TranslationFault,

    /// <summary>
    /// The operation was rejected due to privilege or protection violations (e.g., write-protect, user-mode violation).
    /// </summary>
    AccessViolation
}
