// Avishai Dernis 2026

namespace Zarem.Mips.Emulator.Machine.Enums;

/// <summary>
/// Specifies the Cache Coherency Attributes (CCA) for a MIPS TLB page mapping.
/// Determines how the CPU cache and memory bus interact with the translated virtual page.
/// </summary>
public enum MipsCacheAttribute : byte
{
    /// <summary>
    /// Cacheable, noncoherent, write-through, no write allocate.
    /// commonly used for standard read-mostly sections on older MIPS variants.
    /// </summary>
    CacheableNoncoherentWriteThrough = 0,

    /// <summary>
    /// Cacheable, noncoherent, write-back.
    /// Provides high performance by updating the cache line first and deferring memory updates.
    /// </summary>
    CacheableNoncoherentWriteBack = 1,

    /// <summary>
    /// Uncached. Bypasses all cache levels completely. 
    /// Mandatory for Memory-Mapped I/O (MMIO) and peripheral registers to avoid reading stale states.
    /// </summary>
    Uncached = 2,

    /// <summary>
    /// Cacheable, noncoherent. Standard mode for normal application memory 
    /// and operating system kernels running on single-core setups (e.g., VR4300 / N64).
    /// </summary>
    CacheableNoncoherent = 3,

    /// <summary>
    /// Cacheable, coherent, exclusive. Used in multi-core environments 
    /// to signal that a core owns the exclusive dirty state of a page line.
    /// </summary>
    CacheableCoherentExclusive = 4,

    /// <summary>
    /// Cacheable, coherent, exclusive on write. Inter-core synchronization state 
    /// ensuring other processors invalidate their matching lines upon write entry.
    /// </summary>
    CacheableCoherentExclusiveOnWrite = 5,

    /// <summary>
    /// Cacheable, coherent, update on write. Broadcasts modified cache states 
    /// across the shared multiprocessor bus to update sibling lines concurrently.
    /// </summary>
    CacheableCoherentUpdateOnWrite = 6,

    /// <summary>
    /// Uncached Accelerated. Gathers sequential byte/word writes into a temporary 
    /// write buffer to execute unified burst memory cycles. Ideal for video framebuffers.
    /// </summary>
    UncachedAccelerated = 7
}
