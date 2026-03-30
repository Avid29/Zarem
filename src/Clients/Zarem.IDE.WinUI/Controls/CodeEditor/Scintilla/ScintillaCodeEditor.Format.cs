// Avishai Dernis 2026

using System.Collections.Generic;
using System.Text;
using Windows.UI;
using WinUIEditor;
using Zarem.Assembler.Tokenization;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Assembler.Tokenization.Models.Enums;

namespace Zarem.IDE.Controls.CodeEditor.Scintilla;

public partial class ScintillaCodeEditor
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

    // 14 is reserved for the line indicators

    private const int ErrorAnnotationStyleIndex = 17;
    private const int WarningAnnotationStyleIndex = 18;
    private const int MessageAnnotationStyleIndex = 19;

    private void SetUpHighlighting()
    {
        if (!TryGetEditor(out var editor))
            return;

        if (ColorScheme is null)
            return;

        editor.StyleSetFore(InstructionStyleIndex, ToInt(ColorScheme.InstructionHighlightColor));
        editor.StyleSetFore(RegisterStyleIndex, ToInt(ColorScheme.RegisterHighlightColor));
        editor.StyleSetFore(ImmediateStyleIndex, ToInt(ColorScheme.ImmediateHighlightColor));
        editor.StyleSetFore(ReferenceStyleIndex, ToInt(ColorScheme.ReferenceHighlightColor));
        editor.StyleSetFore(OperatorStyleIndex, ToInt(ColorScheme.OperatorHighlightColor));
        editor.StyleSetFore(DirectiveStyleIndex, ToInt(ColorScheme.DirectiveHighlightColor));
        editor.StyleSetFore(StringStyleIndex, ToInt(ColorScheme.StringHighlightColor));
        editor.StyleSetFore(CommentStyleIndex, ToInt(ColorScheme.CommentHighlightColor));

        editor.StyleSetFore(ErrorAnnotationStyleIndex, ToInt(ColorScheme.ErrorUnderlineColor));
        editor.StyleSetFore(WarningAnnotationStyleIndex, ToInt(ColorScheme.WarningUnderlineColor));
        editor.StyleSetFore(MessageAnnotationStyleIndex, ToInt(ColorScheme.MessageUnderlineColor));
    }

    private void UpdateSyntaxHighlighting()
    {
        if (!TryGetEditor(out var editor))
            return;

        // Clear the style
        editor.StartStyling(0, 0);
        editor.SetStyling(editor.Length, 0);

        Stack<string> foldLabels = new();
        for(long i = 0; i < editor.LineCount; i++)
        {
            // TODO: Check if the line has been updated
            FormatLine(i, foldLabels);
        }
    }

    private void FormatLine(long line, Stack<string> foldLabels)
    {
        if (!TryGetEditor(out var editor))
            return;

        // We need to convert everything to utf8 sizing
        var text = editor.GetLine(line).Trim('\n', '\r');

        // Tokenize the line
        var tokenized = Tokenizer.TokenizeLine(text, mode: TokenizerMode.IDE);

        // Track the position
        long pos = GetMappedIndex(line, 0);

        // Apply Syntax Highlighting for each token
        foreach (var asmLine in tokenized)
        {
            foreach (var token in asmLine.Tokens)
            {
                var style = token.Type switch
                {
                    TokenType.Instruction => InstructionStyleIndex,
                    TokenType.Register => RegisterStyleIndex,
                    TokenType.Immediate => ImmediateStyleIndex,

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
                var tokenLength = Encoding.UTF8.GetByteCount(token.Source);

                editor.StartStyling(pos, 0);
                editor.SetStyling(tokenLength, style);
                pos += tokenLength;
            }
        }

        // Adjust fold level based on the line's label
        var foldLevel = GetAndAdjustLabelDepth(tokenized[0].Label, foldLabels);
        editor.SetFoldLevel(line, foldLevel);
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
        while (labels.TryPeek(out var current))
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
        return (FoldLevel)(labels.Count - 1) | FoldLevel.HeaderFlag | FoldLevel.Base;
    }

    private static int ToInt(Color color) => color.R | color.G << 8 | color.B << 16 | color.A << 24;
}
