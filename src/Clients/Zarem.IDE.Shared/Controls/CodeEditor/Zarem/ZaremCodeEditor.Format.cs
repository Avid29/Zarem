// Avishai Dernis 2026

using Microsoft.UI;
using Microsoft.UI.Text;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Assembler.Tokenization;
using Zarem.Assembler.Tokenization.Models.Enums;

namespace Zarem.IDE.Controls.CodeEditor.Zarem;

public partial class ZaremCodeEditor
{
    private bool @lock = false;

    /// <summary>
    /// Applies formatting based on a log messages.
    /// </summary>
    public void ApplyLogHighlights(IReadOnlyList<ILog> logs)
    {
        Document.BatchDisplayUpdates();

        // Clear underlines
        Document.GetText(TextGetOptions.None, out var temp);
        var range = Document.GetRange(0, temp.Length - 1);
        range.CharacterFormat.Underline = UnderlineType.None;

        //foreach (var log in logs)
        //{
        //    // Get log range
        //    range = Document.GetRange(log, log);

        //    // Underline range
        //    range.CharacterFormat.Underline = log.Severity switch
        //    {
        //        Severity.Message => UnderlineType.ThickDotted,
        //        Severity.Warning => UnderlineType.Wave,
        //        Severity.Error => UnderlineType.Wave,
        //        _ => UnderlineType.Undefined,
        //    };
        //}

        Document.ApplyDisplayUpdates();
    }

    private async Task UpdateSyntaxHighlightingAsync()
    {
        if (@lock)
            return;

        @lock = true;
        Document.GetText(TextGetOptions.None, out string text);

        // Format line by line
        var reader = new StringReader(text);
        int pos = 0;
        while (true)
        {
            var line = await reader.ReadLineAsync();
            if (line is null)
                break;

            // TODO: Check if the line has been updated
            FormatLine(pos, line);
            pos += line.Length + 1;
        }
        @lock = false;
    }

    private void FormatLine(int lineStart, string line)
    {
        if (ColorScheme is null)
            return;

        // Batch the following display updates
        Document.BatchDisplayUpdates();

        // Clear the line to white
        var lineRange = Document.GetRange(lineStart, lineStart + line.Length);
        lineRange.CharacterFormat.ForegroundColor = Colors.White;

        // Tokenize the line
        var tokenized = Tokenizer.TokenizeLine(line, mode: TokenizerMode.IDE);
        foreach (var token in tokenized[0].Tokens)
        {
            var tokenStart = lineStart + token.Location.Column;
            var tokenEnd = tokenStart + token.Source.Length;
            var tokenDocumentRange = Document.GetRange(tokenStart, tokenEnd);

            tokenDocumentRange.CharacterFormat.ForegroundColor = token.Type switch
            {
                TokenType.Instruction => ColorScheme.InstructionHighlightColor,
                TokenType.Register => ColorScheme.RegisterHighlightColor,
                TokenType.Immediate => ColorScheme.ImmediateHighlightColor,

                TokenType.Reference or
                TokenType.LabelDeclaration => ColorScheme.ReferenceHighlightColor,

                TokenType.Operator => ColorScheme.OperatorHighlightColor,

                TokenType.Directive => ColorScheme.DirectiveHighlightColor,
                TokenType.String => ColorScheme.StringHighlightColor,
                TokenType.Comment => ColorScheme.CommentHighlightColor,
                _ => Colors.White,
            };
        }

        // Apply display updates
        Document.ApplyDisplayUpdates();
    }
}
