// Avishai Dernis 2026

using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Zarem.Assembler;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Logging.Enum;
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
    public void ApplyLogHighlights(IReadOnlyList<AssemblerEntry> logs)
    {
        if (_diagnosticOverlay is null)
            return;

        _diagnosticOverlay.Children.Clear();

        foreach (var log in logs)
        {
            if (log.Location is null)
                continue;

            // Get the range of the log's tokens
            int start = (int)log.Location.Value.Index;
            int length = log.Tokens.Sum(t => t.Source.Length);
            var range = Document.GetRange(start, start + length);

            // Get the range's rectangle
            range.GetRect(PointOptions.ClientCoordinates | PointOptions.NoHorizontalScroll, out Rect rect, out _);

            if (rect.Width <= 0)
                continue;

            var squiggle = CreateSquigglePath(rect.Width, log.Severity);

            Canvas.SetLeft(squiggle, rect.Left);
            Canvas.SetTop(squiggle, rect.Bottom - 2);

            _diagnosticOverlay.Children.Add(squiggle);
        }
    }

    private FrameworkElement CreateSquigglePath(double width, Severity severity)
    {
        var polyline = new Polyline
        {
            Stroke = severity == Severity.Error ? new SolidColorBrush(Colors.Red)
                                                : new SolidColorBrush(Colors.Orange),
            StrokeThickness = 1,
            Points = []
        };

        // Generate a simple zig-zag wave
        double step = 2;        // Width of one zig
        double amplitude = 2;   // Height of the zig
        for (double x = 0; x <= width; x += step)
        {
            polyline.Points.Add(new Point(x, (x / step) % 2 == 0 ? 0 : amplitude));
        }

        return polyline;
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
        var tokenized = Tokenizer.TokenizeLine(line, MipsTokenizerProfile.Default, mode: TokenizerMode.IDE);
        foreach (var token in tokenized[0].Tokens)
        {
            var tokenStart = lineStart + token.Location.Column;
            var tokenEnd = tokenStart + token.Source.Length;
            var tokenDocumentRange = Document.GetRange(tokenStart, tokenEnd);

            tokenDocumentRange.CharacterFormat.ForegroundColor = token.Type switch
            {
                TokenType.Instruction => ColorScheme.InstructionHighlightColor,
                TokenType.Register or TokenType.RegisterPrefix => ColorScheme.RegisterHighlightColor,
                TokenType.Immediate or TokenType.ImmediatePrefix => ColorScheme.ImmediateHighlightColor,

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
