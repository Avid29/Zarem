// Avishai Dernis 2025

using Zarem.Assembler.Config;
using Zarem.Assembler.Models;
using Zarem.Assembler.Tokenization;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Assembler.Tokenization.Models.Enums;
using Zarem.Models.Tables;
using Symbol = Zarem.Models.Tables.Symbol;

namespace Zarem.WinUI.Controls.CodeEditor;

public partial class AssemblyEditor
{
    private const int InstructionStyleIndex = 1;
    private const int RegisterStyleIndex = 2;
    private const int ImmediateStyleIndex = 3;
    private const int ReferenceStyleIndex = 4;
    private const int OperatorStyleIndex = 5;
    private const int DirectiveStyleIndex = 6;
    private const int StringStyleIndex = 7;
    private const int CommentStyleIndex = 8;
    private const int MacroStyleIndex = 9;
    private const int InvalidInstructionStyleIndex = 10;
    //private const int InvalidRegisterStyleIndex = 11;
    private const int InvalidReferenceStyleIndex = 12;

    // 14 is reserved for the line indicators
    private const int ErrorAnnotationStyleIndex = 17;
    private const int WarningAnnotationStyleIndex = 18;
    private const int MessageAnnotationStyleIndex = 19;

    private HashSet<string>? Instructions = null;
    private HashSet<string>? Symbols = null;

    private bool @lock = false;

    private void SetupHighlighting()
    {

    }

    private void SetupKeywords()
    {
        Instructions = [];

        // Get the instruction table
        var config = AssemblerConfig ?? new MIPSAssemblerConfig();
        var table = new InstructionTable(config);
        var instructions = table.GetInstructions();

        foreach (var instr in instructions)
        {
            // TODO: Handle formatting instructions
            Instructions.Add(instr.Name);
        }
    }

    private void UpdateSymbols(IReadOnlyList<Symbol> symbols)
    {
        Symbols = [];

        foreach(var symbol in symbols)
            Symbols.Add(symbol.Name);
    }

    private void UpdateSyntaxHighlighting()
    {

    }

    private void FormatLine(ref SourceLocation utf16Pos, ref SourceLocation utf8Pos, string line, Stack<string> foldLabels)
    {

    }
}
