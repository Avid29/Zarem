// Avishai Dernis 2026

using Zarem.RiscV.Emulator.Config;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums.Functions;
using Zarem.RiscV.Models.Instructions.Enums.Operations;
using Zarem.RiscV.Models.Instructions.Enums.Registers;
using Zarem.RiscV.Models.Versioning.Enums;

namespace Zarem.RiscV.Emulator.Helper;

/// <summary>
/// A class for decompressing <see cref="RiscVCompressedInstruction"/> into equivalent <see cref="RiscVInstruction"/> values.
/// </summary>
public class InstructionDecompressor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InstructionDecompressor"/> class.
    /// </summary>
    public InstructionDecompressor(RiscVEmulatorConfig config)
    {
        Config = config;
    }

    /// <summary>
    /// Gets the emulator's config.
    /// </summary>
    public RiscVEmulatorConfig Config { get; }

    /// <summary>
    /// Decompresses a <see cref="RiscVCompressedInstruction"/> into a <see cref="RiscVInstruction"/>.
    /// </summary>
    public bool Decompress(RiscVCompressedInstruction compressed, out RiscVInstruction uncompressed)
    {
        uncompressed = compressed.CompressionCode switch
        {
            RiscVCompressionCode.C0 => DecompressQ0(compressed),
            RiscVCompressionCode.C1 => DecompressQ1(compressed),
            _ => compressed
        };

        // If the instruction is uncompressed, decompression succeeded
        return uncompressed.CompressionCode == RiscVCompressionCode.Uncompressed;
    }

    private RiscVInstruction DecompressQ0(RiscVCompressedInstruction compressed)
    {
        var rd = compressed.RD_Compressed;
        var rs1 = compressed.RS1_Compressed;
        var rs2 = compressed.RS2_Compressed;

        return compressed.Funct3 switch
        {
            // c.addi4spn -> addi
            CFunct3Code.AddImmediate4StackPointerNonDestructive when compressed.StackOffset is not 0 => RiscVInstruction.CreateI(
                RiscVOpCode.OpImmediate, Funct3Code.Arithmetic, rd, RiscVGpRegister.StackPointer, (short)compressed.StackOffset),

            // c.fld -> fld [RVCD]
            CFunct3Code.LoadDouble when Config.VersionInfo.HasExtensions(RiscVExtensions.DoubleFloatingPoint) => RiscVInstruction.CreateI(
                RiscVOpCode.FloatLoad, Funct3Code.LoadDoubleWord, rd, rs1, (short)compressed.DoubleWordLoadStoreOffset),

            // c.lw -> lw 
            CFunct3Code.LoadWord => RiscVInstruction.CreateI(
                RiscVOpCode.Load, Funct3Code.LoadWord, rd, rs1, compressed.WordLoadStoreOffset),

            // c.ld -> ld [RV64C]
            CFunct3Code.LoadDoubleWord when Config.VersionInfo.Base is >= RiscVBaseVersion.RV64 => RiscVInstruction.CreateI(
                RiscVOpCode.Load, Funct3Code.LoadDoubleWord, rd, rs1, (short)compressed.DoubleWordLoadStoreOffset),

            // c.flw -> flw [RVC32F]
            CFunct3Code.LoadSingle when Config.VersionInfo.Base is RiscVBaseVersion.RV32
                && Config.VersionInfo.HasExtensions(RiscVExtensions.SingleFloatingPoint) =>
                    RiscVInstruction.CreateI(RiscVOpCode.FloatLoad, Funct3Code.LoadWord, rd, rs1, compressed.WordLoadStoreOffset),

            // c.fsd -> fsd [RVCD]
            CFunct3Code.StoreDouble when Config.VersionInfo.HasExtensions(RiscVExtensions.DoubleFloatingPoint) => RiscVInstruction.CreateS(
                RiscVOpCode.FloatStore, Funct3Code.StoreDoubleWord, rs1, rs2, (short)compressed.DoubleWordLoadStoreOffset),

            // c.sw -> sw 
            CFunct3Code.StoreWord => RiscVInstruction.CreateS(
                RiscVOpCode.Store, Funct3Code.StoreWord, rs1, rs2, compressed.WordLoadStoreOffset),

            // c.sd -> sd  [RV64C]
            CFunct3Code.StoreDoubleWord when Config.VersionInfo.Base is >= RiscVBaseVersion.RV64 => RiscVInstruction.CreateS(
                RiscVOpCode.Store, Funct3Code.StoreDoubleWord, rs1, rs2, (short)compressed.DoubleWordLoadStoreOffset),

            // c.fsw -> fsw [RV32CF]
            CFunct3Code.StoreSingle when Config.VersionInfo.Base is RiscVBaseVersion.RV32
                && Config.VersionInfo.HasExtensions(RiscVExtensions.SingleFloatingPoint) =>
                    RiscVInstruction.CreateS(RiscVOpCode.FloatStore, Funct3Code.StoreWord, rs1, rs2, compressed.WordLoadStoreOffset),

            // Default to leaving the instruction compressed 
            _ => compressed,
        };
    }

    private RiscVInstruction DecompressQ1(RiscVCompressedInstruction compressed)
    {
        var rdrs1 = compressed.RDRS1;
        var rd = compressed.RD_Compressed;
        var rs1 = compressed.RS1_Compressed;
        var rs2 = compressed.RS2_Compressed;

        return compressed.Funct3 switch
        {
            // c.addi -> addi
            CFunct3Code.AddImmediate => RiscVInstruction.CreateI(
                RiscVOpCode.OpImmediate, Funct3Code.Arithmetic, rdrs1, rdrs1, compressed.Immediate),

            // c.jal -> jal [RV32C]
            CFunct3Code.JumpAndLink when Config.VersionInfo.Base is RiscVBaseVersion.RV32 => RiscVInstruction.CreateJ(
                RiscVOpCode.JumpAndLink, RiscVGpRegister.ReturnAddress, compressed.JumpOffset),

            // c.addiw -> addiw [RV64C] (rd != x0)
            CFunct3Code.AddImmediateWide when Config.VersionInfo.Base is >= RiscVBaseVersion.RV64 && rdrs1 is not RiscVGpRegister.Zero => RiscVInstruction.CreateI(
                RiscVOpCode.OpImmediate32, Funct3Code.Arithmetic, rdrs1, rdrs1, compressed.Immediate),

            // c.li -> addi (rd != x0)
            CFunct3Code.LoadImmediate when rdrs1 is not RiscVGpRegister.Zero => RiscVInstruction.CreateI(
                RiscVOpCode.OpImmediate, Funct3Code.Arithmetic, rdrs1, RiscVGpRegister.Zero, compressed.Immediate),

            // c.addi16sp -> addi (rd == x2 && imm != 0)
            CFunct3Code.AddImmediate16StackPointer when rdrs1 is RiscVGpRegister.StackPointer && compressed.StackStoreOffset is not 0 => RiscVInstruction.CreateI(
                RiscVOpCode.OpImmediate, Funct3Code.Arithmetic, RiscVGpRegister.StackPointer, RiscVGpRegister.StackPointer, compressed.StackStoreOffset),

            // c.lui -> lui (rd != x0, rd != x2, imm != 0)
            CFunct3Code.LoadUpperImmediate when rdrs1 is not (RiscVGpRegister.Zero or RiscVGpRegister.StackPointer) && compressed.StackStoreOffset is not 0 => RiscVInstruction.CreateU(
                RiscVOpCode.LoadUpperImmediate, rdrs1, compressed.Immediate),

            // c.j -> jal
            CFunct3Code.Jump => RiscVInstruction.CreateJ(
                RiscVOpCode.JumpAndLink, RiscVGpRegister.Zero, compressed.JumpOffset),

            // c.beqz -> beq
            CFunct3Code.BranchOnEqualToZero => RiscVInstruction.CreateB(
                RiscVOpCode.Branch, Funct3Code.BranchEqual, rs1, RiscVGpRegister.Zero, compressed.BranchOffset),

            // c.bnez -> bne
            CFunct3Code.BranchOnNotEqualToZero => RiscVInstruction.CreateB(
                RiscVOpCode.Branch, Funct3Code.BranchNotEqual, rd, RiscVGpRegister.Zero, compressed.BranchOffset),

            CFunct3Code.MiscAlu => DecompressQ1Arith(compressed, rs1, rs2),

            _ => compressed,
        };
    }

    private RiscVInstruction DecompressQ1Arith(RiscVCompressedInstruction compressed, RiscVGpRegister rdrs1, RiscVGpRegister rs2)
    {
        return compressed.CBAFunct2 switch
        {
            // c.srli -> srli
            CFunct2Code.ShiftRightLogicalImmediate => RiscVInstruction.CreateI(
                RiscVOpCode.OpImmediate, Funct3Code.ShiftRight, rdrs1, rdrs1, compressed.CBAImmediate),

            // c.srai -> srai
            CFunct2Code.ShiftRightArithmeticImmediate => RiscVInstruction.CreateI(
                RiscVOpCode.OpImmediate, Funct3Code.ShiftRight, rdrs1, rdrs1, compressed.CBAImmediate),

            // c.andi -> andi
            CFunct2Code.AndImmediate => RiscVInstruction.CreateI(
                RiscVOpCode.OpImmediate, Funct3Code.And, rdrs1, rdrs1, compressed.CBAImmediate),

            // R-type subgroup
            _ => (compressed.Funct6, compressed.Funct2) switch
            {
                // c.sub -> sub
                (CFunct6Code.ArithmeticLogic, CFunct2Code.Subtract) => RiscVInstruction.CreateR(
                    RiscVOpCode.Op, Funct3Code.Arithmetic, Funct7Code.Modified, rdrs1, rdrs1, rs2),

                // c.xor -> xor
                (CFunct6Code.ArithmeticLogic, CFunct2Code.Xor) => RiscVInstruction.CreateR(
                    RiscVOpCode.Op, Funct3Code.Xor, Funct7Code.Base, rdrs1, rdrs1, rs2),

                // c.or -> or
                (CFunct6Code.ArithmeticLogic, CFunct2Code.Or) => RiscVInstruction.CreateR(
                    RiscVOpCode.Op, Funct3Code.Or, Funct7Code.Base, rdrs1, rdrs1, rs2),

                // c.and -> and
                (CFunct6Code.ArithmeticLogic, CFunct2Code.And) => RiscVInstruction.CreateR(
                    RiscVOpCode.Op, Funct3Code.And, Funct7Code.Base, rdrs1, rdrs1, rs2),

                // c.subw -> sub [RV64C]
                (CFunct6Code.ArithmeticLogicW, CFunct2Code.Subtract) when Config.VersionInfo.Base >= RiscVBaseVersion.RV64 => RiscVInstruction.CreateR(
                    RiscVOpCode.Op32, Funct3Code.Arithmetic, Funct7Code.Modified, rdrs1, rdrs1, rs2),

                // c.addw -> add [RV64C]
                (CFunct6Code.ArithmeticLogicW, CFunct2Code.Subtract) when Config.VersionInfo.Base >= RiscVBaseVersion.RV64 => RiscVInstruction.CreateR(
                    RiscVOpCode.Op32, Funct3Code.Arithmetic, Funct7Code.Base, rdrs1, rdrs1, rs2),

                _ => compressed,
            },
        };
    }
}
