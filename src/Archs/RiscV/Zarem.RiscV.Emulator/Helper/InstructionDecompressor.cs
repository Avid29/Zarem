// Avishai Dernis 2026

using System;
using Zarem.RiscV.Emulator.Config;
using Zarem.RiscV.Models.Instructions;
using Zarem.RiscV.Models.Instructions.Enums.Functions;
using Zarem.RiscV.Models.Instructions.Enums.Operations;
using Zarem.RiscV.Models.Instructions.Enums.Registers;
using Zarem.RiscV.Models.Versioning.Enums;

namespace Zarem.RiscV.Emulator.Helper;

/// <summary>
/// A class for decompressing <see cref="RiscVCompressedInstruction"/> into equivilent <see cref="RiscVInstruction"/> values.
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
    public RiscVInstruction Decompress(RiscVCompressedInstruction compressed)
    {
        return compressed.CompressionCode switch
        {
            RiscVCompressionCode.C0 => DecompressQ0(compressed),
            _ => throw new InvalidOperationException($"Invalid compressed instruction opcode quadrant: {compressed.CompressionCode}"),
        };
    }

    private RiscVInstruction DecompressQ0(RiscVCompressedInstruction compressed)
    {
        var rd = compressed.RD_Compressed;
        var rs1 = compressed.RS1_Compressed;
        var rs2 = compressed.RS2_Compressed;

        return compressed.Funct3 switch
        {
            // c.addi4spn -> addi
            CFunct3Code.AddImmediate4StackPointerNonDestructive => RiscVInstruction.CreateI(
                RiscVOpCode.OpImmediate, Funct3Code.Arithmetic, rd, RiscVGpRegister.StackPointer, (short)compressed.StackOffset),

            // c.fld -> fld [RVD]
            CFunct3Code.LoadDouble when Config.VersionInfo.HasExtensions(RiscVExtensions.DoubleFloatingPoint) => RiscVInstruction.CreateI(
                RiscVOpCode.FloatLoad, Funct3Code.LoadDoubleWord, rd, rs1, (short)compressed.DoubleWordLoadStoreOffset),

            // c.lw -> lw 
            CFunct3Code.LoadWord => RiscVInstruction.CreateI(
                RiscVOpCode.Load, Funct3Code.LoadWord, rd, rs1, compressed.WordLoadStoreOffset),

            // c.ld -> ld [RV64]
            CFunct3Code.LoadDoubleWord when Config.VersionInfo.Base is >= RiscVBaseVersion.RV64 => RiscVInstruction.CreateI(
                RiscVOpCode.Load, Funct3Code.LoadDoubleWord, rd, rs1, (short)compressed.DoubleWordLoadStoreOffset),

            // c.flw -> flw [RV32F]
            CFunct3Code.LoadSingle when Config.VersionInfo.Base is RiscVBaseVersion.RV32
                && Config.VersionInfo.HasExtensions(RiscVExtensions.SingleFloatingPoint) =>
                    RiscVInstruction.CreateI(RiscVOpCode.FloatLoad, Funct3Code.LoadWord, rd, rs1, compressed.WordLoadStoreOffset),

            // c.fsd -> fsd [RVD]
            CFunct3Code.StoreDouble when Config.VersionInfo.HasExtensions(RiscVExtensions.DoubleFloatingPoint) => RiscVInstruction.CreateS(
                RiscVOpCode.FloatStore, Funct3Code.StoreDoubleWord, rs1, rs2, (short)compressed.DoubleWordLoadStoreOffset),

            // c.sw -> sw 
            CFunct3Code.StoreWord => RiscVInstruction.CreateS(
                RiscVOpCode.Store, Funct3Code.StoreWord, rs1, rs2, compressed.WordLoadStoreOffset),

            // c.sd -> sd  [RV64]
            CFunct3Code.StoreDoubleWord when Config.VersionInfo.Base is >= RiscVBaseVersion.RV64 => RiscVInstruction.CreateS(
                RiscVOpCode.Store, Funct3Code.StoreDoubleWord, rs1, rs2, (short)compressed.DoubleWordLoadStoreOffset),

            // c.fsw -> fsw [RV32F]
            CFunct3Code.StoreSingle when Config.VersionInfo.Base is RiscVBaseVersion.RV32
                && Config.VersionInfo.HasExtensions(RiscVExtensions.SingleFloatingPoint) =>
                    RiscVInstruction.CreateS(RiscVOpCode.FloatStore, Funct3Code.StoreWord, rs1, rs2, compressed.WordLoadStoreOffset),

            _ => throw new InvalidOperationException($"Unsupported Q0 compressed funct3 opcode: {compressed.Funct3}"),
        };
    }
}
