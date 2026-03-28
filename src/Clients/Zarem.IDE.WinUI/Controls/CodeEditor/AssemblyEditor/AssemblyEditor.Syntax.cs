// Avishai Dernis 2025

using System.Collections.Generic;
using System.IO;
using WinUIEditor;
using Zarem.Assembler.Config;
using Zarem.Assembler.Helpers.Tables;
using Zarem.Assembler.Models;
using Zarem.Assembler.Tokenization;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Assembler.Tokenization.Models.Enums;
using Zarem.Models.Tables;

namespace Zarem.IDE.Controls.CodeEditor;

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


    private InstructionTable? _instructionTable = null;
    private HashSet<string>? _symbolsSet = null;

    private bool @lock = false;

    private void SetupHighlighting()
    {
        if (!TryGetEditor(out var editor))
            return;

        editor.StyleSetFore(InstructionStyleIndex, ToInt(SyntaxHighlightingTheme.InstructionHighlightColor));
        editor.StyleSetFore(RegisterStyleIndex, ToInt(SyntaxHighlightingTheme.RegisterHighlightColor));
        editor.StyleSetFore(ImmediateStyleIndex, ToInt(SyntaxHighlightingTheme.ImmediateHighlightColor));
        editor.StyleSetFore(ReferenceStyleIndex, ToInt(SyntaxHighlightingTheme.ReferenceHighlightColor));
        editor.StyleSetFore(OperatorStyleIndex, ToInt(SyntaxHighlightingTheme.OperatorHighlightColor));
        editor.StyleSetFore(DirectiveStyleIndex, ToInt(SyntaxHighlightingTheme.DirectiveHighlightColor));
        editor.StyleSetFore(StringStyleIndex, ToInt(SyntaxHighlightingTheme.StringHighlightColor));
        editor.StyleSetFore(CommentStyleIndex, ToInt(SyntaxHighlightingTheme.CommentHighlightColor));

        editor.StyleSetFore(InvalidInstructionStyleIndex, ToInt(SyntaxHighlightingTheme.InvalidInstructionHighlightColor));
        editor.StyleSetFore(InvalidReferenceStyleIndex, ToInt(SyntaxHighlightingTheme.InvalidReferenceHighlightColor));

        editor.StyleSetFore(ErrorAnnotationStyleIndex, ToInt(SyntaxHighlightingTheme.ErrorUnderlineColor));
        editor.StyleSetFore(WarningAnnotationStyleIndex, ToInt(SyntaxHighlightingTheme.WarningUnderlineColor));
        editor.StyleSetFore(MessageAnnotationStyleIndex, ToInt(SyntaxHighlightingTheme.MessageUnderlineColor));
    }

    private void SetupKeywords()
    {
        // Get the instruction table
        var config = AssemblerConfig ?? new MipsAssemblerConfig();
        _instructionTable = new InstructionTable(config);
    }

    private void UpdateSymbols(IReadOnlyList<Symbol> symbols)
    {
        _symbolsSet = [];

        foreach(var symbol in symbols)
            _symbolsSet.Add(symbol.Name);
    }

    private void UpdateSyntaxHighlighting()
    {
        if (@lock || ChildEditor is null)
            return;

        @lock = true;
        var editor = ChildEditor.Editor;

        // Clear the style
        editor.StartStyling(0, 0);
        editor.SetStyling(editor.Length, 0);

        // Clear token mappings
        _locationMapper.Clear();

        // Format line by line
        var text = editor.GetText(editor.Length);
        var reader = new StringReader(text);
        var utf16Pos = new SourceLocation();
        var utf8Pos = new SourceLocation();

        Stack<string> foldLabels = new();
        while (true)
        {
            var line = reader.ReadLine();
            if (line is null)
                break;

            // TODO: Check if the line has been updated
            FormatLine(ref utf16Pos, ref utf8Pos, line, foldLabels);
            utf8Pos = utf8Pos.NextLine(2);
            utf16Pos = utf16Pos.NextLine(1);
        }

        @lock = false;
    }

    private void FormatLine(ref SourceLocation utf16Pos, ref SourceLocation utf8Pos, string line, Stack<string> foldLabels)
    {
        if (!TryGetEditor(out var editor))
            return;

        // We need to convert everything to utf8 sizing
        var lineLength = GetEncodingSize(line);

        // Clear the line to white
        editor.StartStyling(utf8Pos.Index, 0);
        editor.SetStyling(lineLength, 0);

        // Tokenize the line
        var tokenized = Tokenizer.TokenizeLine(line, mode: TokenizerMode.IDE);

        // Apply Syntax Highlighting for each token
        foreach (var asmLine in tokenized)
        {
            foreach (var token in asmLine.Tokens)
            {
                // Log token position in location mapper.
                _locationMapper.RegisterReferencePoint(utf16Pos.Index, utf8Pos.Index);

                var style = token.Type switch
                {
                    TokenType.Instruction when _instructionTable is not null =>
                        _instructionTable.TryGetInstruction(FloatFormatTable.TryGetFloatFormat(token.Source, out _, out var lookup) ? lookup : token.Source, out _, out _, out _, out var banned) ? InstructionStyleIndex : InvalidInstructionStyleIndex,

                    TokenType.Instruction => InstructionStyleIndex,
                    TokenType.Register => RegisterStyleIndex,
                    TokenType.Immediate => ImmediateStyleIndex,

                    TokenType.Reference when _symbolsSet is not null =>
                        _symbolsSet.Contains(token.Source) ? ReferenceStyleIndex : InvalidReferenceStyleIndex,

                    TokenType.Reference or
                    TokenType.LabelDeclaration => ReferenceStyleIndex,

                    TokenType.OpenParenthesis or TokenType.CloseParenthesis or
                    TokenType.OpenBracket or TokenType.CloseBracket or TokenType.Comma or
                    TokenType.Operator => OperatorStyleIndex,

                    TokenType.Directive => DirectiveStyleIndex,
                    TokenType.String => StringStyleIndex,
                    TokenType.Comment => CommentStyleIndex,

                    _ => 0,
                };

                // Set style and advance utf8/utf16 positions
                //var tokenLength = Encoding.UTF8.GetByteCount(token.Source);
                var tokenLengthUtf8 = GetEncodingSize(token.Source);
                var tokenLengthUtf16 = token.Source.Length;
                editor.StartStyling(utf8Pos.Index, 0);
                editor.SetStyling(tokenLengthUtf8, style);
                utf8Pos += tokenLengthUtf8;
                utf16Pos += tokenLengthUtf16;
            }
        }

        // Adjust fold level based on the line's label
        var foldLevel = GetAndAdjustLabelDepth(tokenized[0].Label, foldLabels);
        editor.SetFoldLevel(utf8Pos.Line, foldLevel);
    }

    private static FoldLevel GetAndAdjustLabelDepth(Token? label, Stack<string> labels)
    {
        // No adjustments to make
        if (label is null)
        {
            return (FoldLevel)labels.Count | FoldLevel.Base;
        }

        var name = label.Source.TrimEnd(':');

        // Search for parent or partner label
        while(labels.TryPeek(out var current))
        {
            if (name.StartsWith($"{current}_"))
                break;

            labels.Pop();
        }

        // Pop the label to end it
        if (labels.TryPeek(out var top) && name == $"{top}_end")
        {
            labels.Pop();
            return (FoldLevel)labels.Count + 1 | FoldLevel.Base;
        }

        // Push child label
        labels.Push(name);
        return (FoldLevel)(labels.Count-1) | FoldLevel.HeaderFlag | FoldLevel.Base;
    }
}
