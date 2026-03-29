// Avishai Dernis 2026

using Microsoft.UI;
using Microsoft.UI.Text;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Zarem.Assembler.Logging.Interfaces;

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
        // Batch the following display updates
        Document.BatchDisplayUpdates();

        // Clear the line to white
        var lineRange = Document.GetRange(lineStart, lineStart + line.Length);
        lineRange.CharacterFormat.ForegroundColor = Colors.White;

        // Tokenize the line
        //var tokenized = Tokenizer.TokenizeLine(line, mode: TokenizerMode.IDE);
        //foreach (var token in tokenized.Tokens)
        //{
        //    var tokenStart = lineStart + token.Location.Column - 1;
        //    var tokenEnd = tokenStart + token.Source.Length;
        //    var tokenDocumentRange = Document.GetRange(tokenStart, tokenEnd);

        //    tokenDocumentRange.CharacterFormat.ForegroundColor = token.Type switch
        //    {
        //        TokenType.Instruction => "#A7FA95".ToColor(),
        //        TokenType.Register => "#FE8482".ToColor(),
        //        TokenType.Immediate => "#F8FC8B".ToColor(),

        //        TokenType.Reference or
        //        TokenType.LabelDeclaration => "#73EEFD".ToColor(),

        //        TokenType.Operator => "#77A7FD".ToColor(),

        //        TokenType.Directive => "#FA9EF6".ToColor(),
        //        TokenType.Comma => "#77A7FD".ToColor(),
        //        TokenType.String => "#FFC47A".ToColor(),
        //        TokenType.Comment => "#9B88FC".ToColor(),
        //        _ => Colors.White,
        //    };
        //}

        // Apply display updates
        Document.ApplyDisplayUpdates();
    }
}
