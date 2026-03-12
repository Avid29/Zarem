// Avishai Dernis 2025

using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Zarem.Assembler.Helpers.Tables;
using Zarem.Helpers.Instructions;
using Zarem.Models.Instructions.Enums;
using Zarem.Models.Instructions.Enums.Operations;
using Zarem.Models.Instructions.Enums.SpecialFunctions;
using Zarem.Models.Instructions.Enums.SpecialFunctions.CoProc0;
using Zarem.Models.Instructions.Enums.SpecialFunctions.FloatProc;

namespace Zarem.Assembler.Models;

/// <summary>
/// A struct containing info on how an instruction is encoded.
/// </summary>
/// <remarks>
/// This struct is JSON Serializable.
/// </remarks>
public readonly struct MipsInstructionMetadata
{
    private static readonly MipsVersion[] AllVersions = [MipsVersion.MipsI, MipsVersion.MipsII, MipsVersion.MipsIII, MipsVersion.MipsIV, MipsVersion.MipsV, MipsVersion.MipsVI];

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsInstructionMetadata"/> struct.
    /// </summary>
    public MipsInstructionMetadata(string name, OperationCode opCode, Argument[] argumentPattern, params MipsVersion[] versions)
    {
        Name = name;
        OpCode = opCode;
        ArgumentPattern = argumentPattern;

        if (versions.Length is 0)
            versions = AllVersions;

        MipsVersions = [..versions];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsInstructionMetadata"/> struct.
    /// </summary>
    public MipsInstructionMetadata(string name, FunctionCode funcCode, Argument[] argumentPattern, params MipsVersion[] versions)
    {
        Name = name;
        OpCode = OperationCode.Special;
        FuncCode = funcCode;
        ArgumentPattern = argumentPattern;

        if (versions.Length is 0)
            versions = AllVersions;

        MipsVersions = [..versions];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsInstructionMetadata"/> struct.
    /// </summary>
    public MipsInstructionMetadata(string name, Func2Code funcCode, Argument[] argumentPattern, params MipsVersion[] versions)
    {
        Name = name;
        OpCode = OperationCode.Special2;
        Function2Code = funcCode;
        ArgumentPattern = argumentPattern;

        if (versions.Length is 0)
            versions = AllVersions;
        
        MipsVersions = [..versions];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsInstructionMetadata"/> struct.
    /// </summary>
    public MipsInstructionMetadata(string name, RegImmFuncCode branchCode, Argument[] argumentPattern, params MipsVersion[] versions)
    {
        Name = name;
        OpCode = OperationCode.RegisterImmediate;
        RegisterImmediateFuncCode = branchCode;
        ArgumentPattern = argumentPattern;

        if (versions.Length is 0)
            versions = AllVersions;
        
        MipsVersions = [..versions];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MipsInstructionMetadata"/> struct.
    /// </summary>
    /// <remarks>
    /// This is constructor is only for pseudo-instructions.
    /// </remarks>
    public MipsInstructionMetadata(string name, PseudoOp pseudoOp, Argument[] argumentPattern, int realizedCount, params MipsVersion[] versions)
    {
        Name = name;
        PseudoOp = pseudoOp;
        ArgumentPattern = argumentPattern;
        RealizedInstructionCount = realizedCount;

        if (versions.Length is 0)
            versions = AllVersions;
        
        MipsVersions = [..versions];
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MipsInstructionMetadata"/> struct.
    /// </summary>
    [JsonConstructor]
    internal MipsInstructionMetadata(
        string name,
        OperationCode? opCode,
        FunctionCode? funcCode,
        Func2Code? function2Code,
        RegImmFuncCode? registerImmediateFuncCode,
        CoProc0RSCode? coProc0RS,
        CoProc1RSCode? coProc1RS,
        Co0FuncCode? co0FuncCode,
        MFMC0FuncCode? mfmc0FuncCode,
        FloatFuncCode? floatFuncCode,
        HashSet<FloatFormat>? floatFormats,
        byte? rs,
        byte? rt,
        byte? rd,
        PseudoOp? pseudoOp,
        Argument[] argumentPattern,
        int? realizedInstructionCount,
        HashSet<MipsVersion> mIPSVersions,
        bool obsolete,
        string behavior)
    {
        Name = name;
        OpCode = opCode;
        FuncCode = funcCode;
        Function2Code = function2Code;
        RegisterImmediateFuncCode = registerImmediateFuncCode;
        CoProc0RS = coProc0RS;
        CoProc1RS = coProc1RS;
        Co0FuncCode = co0FuncCode;
        Mfmc0FuncCode = mfmc0FuncCode;
        FloatFuncCode = floatFuncCode;
        FloatFormats = floatFormats;
        RS = rs;
        RT = rt;
        RD = rd;
        PseudoOp = pseudoOp;
        ArgumentPattern = argumentPattern;
        RealizedInstructionCount = realizedInstructionCount;
        MipsVersions = mIPSVersions;
        Obsolete = obsolete;
        Behavior = behavior;
    }

    /// <summary>
    /// Gets the name of the instruction.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string Name { get; }

    /// <summary>
    /// Gets the instruction operation code.
    /// </summary>
    [JsonPropertyName("op_code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OperationCode? OpCode { get; }

    /// <summary>
    /// Gets the instruction function code.
    /// </summary>
    [JsonPropertyName("func_code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FunctionCode? FuncCode { get; }

    /// <summary>
    /// Gets the instruction function code.
    /// </summary>
    [JsonPropertyName("func2_code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Func2Code? Function2Code { get; }

    /// <summary>
    /// Gets the instruction function code.
    /// </summary>
    [JsonPropertyName("func3_code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Func3Code? Function3Code { get; }

    /// <summary>
    /// Gets the instruction register immediate code.
    /// </summary>
    [JsonPropertyName("rt_func_code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RegImmFuncCode? RegisterImmediateFuncCode { get; }
    
    /// <summary>
    /// Gets the instruction rs function code for a coproc0 instruction.
    /// </summary>
    [JsonPropertyName("coproc0_rs")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CoProc0RSCode? CoProc0RS { get; }
    
    /// <summary>
    /// Gets the instruction rs function code for a float instruction.
    /// </summary>
    [JsonPropertyName("coproc1_rs")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CoProc1RSCode? CoProc1RS { get; }

    /// <summary>
    /// Gets the instruction coprocessor0 function code.
    /// </summary>
    [JsonPropertyName("co0_func_code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Co0FuncCode? Co0FuncCode { get; }

    /// <summary>
    /// Gets the instruction mfmc0 function code.
    /// </summary>
    [JsonPropertyName("mfmc0_func_code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MFMC0FuncCode? Mfmc0FuncCode { get; }

    /// <summary>
    /// Gets the instruction float function code.
    /// </summary>
    [JsonPropertyName("float_func_code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FloatFuncCode? FloatFuncCode { get; }

    /// <summary>
    /// Gets the instruction float function code.
    /// </summary>
    [JsonPropertyName("float_formats")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public HashSet<FloatFormat>? FloatFormats { get; }

    /// <summary>
    /// Gets the provided RS value.
    /// </summary>
    [JsonPropertyName("rs")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public byte? RS { get; }

    /// <summary>
    /// Gets the provided RT value.
    /// </summary>
    [JsonPropertyName("rt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public byte? RT { get; }

    /// <summary>
    /// Gets the provided RD value.
    /// </summary>
    [JsonPropertyName("rd")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public byte? RD { get; }

    /// <summary>
    /// Gets the pseudo op for a pseudo-instruction.
    /// </summary>
    [JsonPropertyName("pseudo_op")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PseudoOp? PseudoOp { get; }

    /// <summary>
    /// Gets the instruction parse type
    /// </summary>
    [JsonPropertyName("args")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public Argument[] ArgumentPattern { get; }

    /// <summary>
    /// Gets the number of real instructions required to execute the instruction.
    /// </summary>
    /// <remarks>
    /// This exists for pseudo instructions.
    /// </remarks>
    [JsonPropertyName("real_instruction_count")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? RealizedInstructionCount { get; }

    /// <summary>
    /// Gets whether or not the instruction is a pseudo instruction.
    /// </summary>
    [JsonIgnore]
    public bool IsPseudoInstruction => !OpCode.HasValue;

    /// <summary>
    /// Gets the version of MIPS required for the instruction.
    /// </summary>
    [JsonPropertyName("versions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public HashSet<MipsVersion> MipsVersions { get; }

    /// <summary>
    /// Gets whether or not the instruction has been marked for removal in a future version.
    /// </summary>
    [JsonPropertyName("obsolete")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Obsolete { get; }

    /// <summary>
    /// Gets a string describing the behavior of the instruction.
    /// </summary>
    [JsonPropertyName("behavior")]
    public string? Behavior { get; }

    /// <summary>
    /// Gets the function's type.
    /// </summary>
    [JsonIgnore]
    public InstructionType Type => InstructionTypeHelper.GetInstructionType(OpCode, RegisterImmediateFuncCode, CoProc0RS ?? (CoProc0RSCode?)CoProc1RS);

    /// <summary>
    /// Gets an identifier for the instruction, including the argument pattern.
    /// </summary>
    /// <remarks>
    /// Introduces a variable argument count. Currently this appears to be an adequate approach.
    /// </remarks>
    public string Identifier => $"{Name}:{ArgumentPattern.Length}";

    /// <summary>
    /// Gets a string showing the usage pattern for the instruction.
    /// </summary>
    public string UsagePattern
    {
        get
        {
            StringBuilder pattern = new($"{Name} ");
            for (int i = 0; i < ArgumentPattern.Length; i++)
            {
                pattern.Append(ArgumentTable.GetArgPatternString(ArgumentPattern[i]));

                if (i < ArgumentPattern.Length - 1)
                {
                    pattern.Append(", ");
                }
            }

            return $"{pattern}";
        }
    }
}
