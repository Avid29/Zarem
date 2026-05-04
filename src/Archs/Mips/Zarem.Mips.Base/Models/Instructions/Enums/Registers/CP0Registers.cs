// Avishai Dernis 2024

namespace Zarem.Mips.Models.Instructions.Enums.Registers;

/// <summary>
/// An enum for coprocessor0 registers.
/// </summary>
public enum CP0Registers
{
#pragma warning disable CS1591

    Index = 0,
    Random = 1,
    EntryLo0 = 2,
    EntryLo1 = 3,
    Context = 4,
    PageMask = 5,
    Wired = 6,
    BadVAddr = 8,
    Count = 9,
    EntryHi = 10,
    Compare = 11,
    Status = 12,
    Cause = 13,
    ExceptionPC = 14,
    ProcessorID = 15,
    Configuation = 16,
    LoadLinkedAddress = 17,
    WatchLo = 18,
    WatchHi = 19,
    XContext = 20, // 64-bit
    Debug = 23,
    DEPC = 24,
    PerfCount = 25,
    ErrCtl = 26,
    CacheErr = 27,
    TagLo = 28,
    DataLo = 28,
    TagHi = 29,
    DataHi = 29,
    ErrorEPC = 30,
    DebugExceptionSave = 31,

#pragma warning restore CS1591
}
