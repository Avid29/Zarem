// Avishai Dernis 2026

namespace Zarem.Z80.Models.Instructions.Enums.Operations;

/// <summary>
/// An enum for Z80 instruction op codes.
/// </summary>
public enum Z80OpCode : uint
{
#pragma warning disable CS1591

    // ========================================================================
    // --- 0x00 - 0x3F Block (Misc Base Ops & Immediate Parameter Loads) -----
    // ========================================================================
    NoOperation = 0x00,
    LoadRegisterPairBcWithImmediate16 = 0x01,
    StoreAccumulatorIndirectBc = 0x02,
    IncrementRegisterPairBc = 0x03,
    IncrementRegisterB = 0x04,
    DecrementRegisterB = 0x05,
    LoadRegisterBWithImmediate8 = 0x06,
    RotateLeftAccumulatorCircular = 0x07,
    ExchangeRegisterPairAfAndAlternateAf = 0x08,
    AddRegisterPairBcToHl = 0x09,
    LoadAccumulatorIndirectBc = 0x0A,
    DecrementRegisterPairBc = 0x0B,
    IncrementRegisterC = 0x0C,
    DecrementRegisterC = 0x0D,
    LoadRegisterCWithImmediate8 = 0x0E,
    RotateRightAccumulatorCircular = 0x0F,

    DecrementAndJumpRelativeIfNonZero = 0x10,
    LoadRegisterPairDeWithImmediate16 = 0x11,
    StoreAccumulatorIndirectDe = 0x12,
    IncrementRegisterPairDe = 0x13,
    IncrementRegisterD = 0x14,
    DecrementRegisterD = 0x15,
    LoadRegisterDWithImmediate8 = 0x16,
    RotateLeftAccumulator = 0x17,
    JumpRelativeUnconditional = 0x18,
    AddRegisterPairDeToHl = 0x19,
    LoadAccumulatorIndirectDe = 0x1A,
    DecrementRegisterPairDe = 0x1B,
    IncrementRegisterE = 0x1C,
    DecrementRegisterE = 0x1D,
    LoadRegisterEWithImmediate8 = 0x1E,
    RotateRightAccumulator = 0x1F,

    JumpRelativeIfNoCarry = 0x20,
    LoadRegisterPairHlWithImmediate16 = 0x21,
    StoreIndirect16FromRegisterPairHl = 0x22,
    IncrementRegisterPairHl = 0x23,
    IncrementRegisterH = 0x24,
    DecrementRegisterH = 0x25,
    LoadRegisterHWithImmediate8 = 0x26,
    DecimalAdjustAccumulator = 0x27,
    JumpRelativeIfCarry = 0x28,
    AddRegisterPairHlToHl = 0x29,
    LoadRegisterPairHlFromIndirect16 = 0x2A,
    DecrementRegisterPairHl = 0x2B,
    IncrementRegisterL = 0x2C,
    DecrementRegisterL = 0x2D,
    LoadRegisterLWithImmediate8 = 0x2E,
    InvertAccumulator = 0x2F,

    JumpRelativeIfNonZero = 0x30,
    LoadStackPointerWithImmediate16 = 0x31,
    StoreIndirect16FromAccumulator = 0x32,
    IncrementStackPointer = 0x33,
    IncrementMemoryIndirectHl = 0x34,
    DecrementMemoryIndirectHl = 0x35,
    LoadMemoryIndirectHlWithImmediate8 = 0x36,
    SetCarryFlag = 0x37,
    JumpRelativeIfZero = 0x38,
    AddRegisterPairSpToHl = 0x39,
    LoadAccumulatorFromIndirect16 = 0x3A,
    DecrementStackPointer = 0x3B,
    IncrementRegisterA = 0x3C,
    DecrementRegisterA = 0x3D,
    LoadRegisterAWithImmediate8 = 0x3E,
    ComplementCarryFlag = 0x3F,

    // ========================================================================
    // --- 0x40 - 0x7F Block (Collapsed Register-to-Register Matrix) ---------
    // ========================================================================
    // Standard Z80 layout maps bits [0..2] to source, and bits [3..5] to dest. 
    // The base 0x40 represents an abstract maskable family.
    LoadRegisterFromRegister = 0x40,
    HaltCpu = 0x76,

    // ========================================================================
    // --- 0x80 - 0xBF Block (Collapsed ALU Operation Matrix) ----------------
    // ========================================================================
    // Opcode bits [0..2] specify register targets, while [3..5] isolate the ALU functional group.
    AddRegister = 0x80,
    AddWithCarryRegister = 0x88,
    SubtractRegister = 0x90,
    SubtractWithCarryRegister = 0x98,
    LogicalAndRegister = 0xA0,
    LogicalXorRegister = 0xA8,
    LogicalOrRegister = 0xB0,
    CompareRegister = 0xB8,

    // ========================================================================
    // --- 0xC0 - 0xFF Block (Control Flow, Jumps & IO Operations) -----------
    // ========================================================================
    ReturnIfNonZero = 0xC0,
    PopRegisterPairBc = 0xC1,
    JumpAbsoluteIfNonZero = 0xC2,
    JumpAbsoluteUnconditional = 0xC3,
    CallSubroutineIfNonZero = 0xC4,
    PushRegisterPairBc = 0xC5,
    AddImmediate8 = 0xC6,
    RestartVector00h = 0xC7,
    ReturnIfZero = 0xC8,
    ReturnUnconditional = 0xC9,
    JumpAbsoluteIfZero = 0xCA,
    CallSubroutineIfZero = 0xCC,
    CallSubroutineUnconditional = 0xCD,
    AddWithCarryImmediate8 = 0xCE,
    RestartVector08h = 0xCF,

    ReturnIfNoCarry = 0xD0,
    PopRegisterPairDe = 0xD1,
    JumpAbsoluteIfNoCarry = 0xD2,
    InputFromPortImmediate8 = 0xD3,
    CallSubroutineIfNoCarry = 0xD4,
    PushRegisterPairDe = 0xD5,
    SubtractImmediate8 = 0xD6,
    RestartVector10h = 0xD7,
    ReturnIfCarry = 0xD8,
    ExchangeRegisterPairDeAndHl = 0xD9,
    JumpAbsoluteIfCarry = 0xDA,
    OutputToPortImmediate8 = 0xDB,
    CallSubroutineIfCarry = 0xDC,
    SubtractWithCarryImmediate8 = 0xDE,
    RestartVector18h = 0xDF,

    ReturnIfParityOdd = 0xE0,
    PopRegisterPairHl = 0xE1,
    JumpAbsoluteIfParityOdd = 0xE2,
    ExchangeStackPointerIndirectAndHl = 0xE3,
    CallSubroutineIfParityOdd = 0xE4,
    PushRegisterPairHl = 0xE5,
    LogicalAndImmediate8 = 0xE6,
    RestartVector20h = 0xE7,
    ReturnIfParityEven = 0xE8,
    JumpIndirectHl = 0xE9,
    JumpAbsoluteIfParityEven = 0xEA,
    ExchangeRegisterPairDeAndAlternateDe = 0xEB,
    CallSubroutineIfParityEven = 0xEC,
    LogicalXorImmediate8 = 0xEE,
    RestartVector28h = 0xEF,

    ReturnIfSignPositive = 0xF0,
    PopRegisterPairAf = 0xF1,
    JumpAbsoluteIfSignPositive = 0xF2,
    DisableInterrupts = 0xF3,
    CallSubroutineIfSignPositive = 0xF4,
    PushRegisterPairAf = 0xF5,
    LogicalOrImmediate8 = 0xF6,
    RestartVector30h = 0xF7,
    ReturnIfSignNegative = 0xF8,
    LoadStackPointerFromHl = 0xF9,
    JumpAbsoluteIfSignNegative = 0xFA,
    EnableInterrupts = 0xFB,
    CallSubroutineIfSignNegative = 0xFC,
    CompareImmediate8 = 0xFE,
    RestartVector38h = 0xFF,

    // ========================================================================
    // --- 0xCB Prefix Block (Bitwise Matrix Operations) ----------------------
    // ========================================================================
    // Paralleling the base ALU matrix layout, these use bit [3..5] for operation,
    // [0..2] for target. The specific bit indices [0..7] are resolved parameterally.
    RotateLeftCircularRegister = 0x00CB,
    RotateRightCircularRegister = 0x08CB,
    RotateLeftRegister = 0x10CB,
    RotateRightRegister = 0x18CB,
    ShiftLeftArithmeticRegister = 0x20CB,
    ShiftRightArithmeticRegister = 0x28CB,
    ShiftLeftLogicalRegister = 0x30CB,
    ShiftRightLogicalRegister = 0x38CB,
    TestBit = 0x40CB,
    ResetBit = 0x80CB,
    SetBit = 0xCCCB,

    // ========================================================================
    // --- 0xED Prefix Block (Unique Extended Operations) --------------------
    // ========================================================================
    InputRegisterFromPortC = 0x40ED,
    OutputPortCFromRegister = 0x41ED,
    SubtractWithCarryRegisterPairToHl = 0x42ED,
    StoreIndirect16FromRegisterPair = 0x43ED,
    NegateAccumulator = 0x44ED,
    ReturnFromInterrupt = 0x45ED,
    SetInterruptMode = 0x46ED,
    LoadInterruptVectorOrRefresh = 0x47ED,
    AddWithCarryRegisterPairToHl = 0x4AED,
    LoadRegisterPairFromIndirect16 = 0x4BED,
    ReturnFromNonMaskableInterrupt = 0x4DED,

    // Block Transfer & Search Groups
    LoadAndIncrement = 0xA0ED,
    CompareAndIncrement = 0xA1ED,
    InputAndIncrement = 0xA2ED,
    OutputAndIncrement = 0xA3ED,
    LoadAndDecrement = 0xA8ED,
    CompareAndDecrement = 0xA9ED,
    InputAndDecrement = 0xAAED,
    OutputAndDecrement = 0xABED,

    LoadAndIncrementRepeat = 0xB0ED,
    CompareAndIncrementRepeat = 0xB1ED,
    InputAndIncrementRepeat = 0xB2ED,
    OutputAndIncrementRepeat = 0xB3ED,
    LoadAndDecrementRepeat = 0xB8ED,
    CompareAndDecrementRepeat = 0xB9ED,
    InputAndDecrementRepeat = 0xBAED,
    OutputAndDecrementRepeat = 0xBBED,

#pragma warning restore CS1591
}
