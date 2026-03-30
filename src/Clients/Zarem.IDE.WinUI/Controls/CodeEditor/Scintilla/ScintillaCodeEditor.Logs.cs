// Avishai Dernis 2026

using Microsoft.UI;
using System.Collections.Generic;
using System.Text;
using WinUIEditor;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Logging.Enum;

namespace Zarem.IDE.Controls.CodeEditor.Scintilla;

public sealed partial class ScintillaCodeEditor
{
    private const int ErrorIndicatorIndex = 8;
    private const int WarningIndicatorIndex = 9;
    private const int MessageIndicatorIndex = 10;

    // Debugging
    private const int ExecutingLineIndicatorIndex = 15;

    /// <summary>
    /// Applies formatting based on a log messages.
    /// </summary>
    public void ApplyLogHighlights(IReadOnlyList<AssemblerEntry> logs)
    {
        if (!TryGetEditor(out var editor))
            return;

        // Clear indicators and annotations
        ClearLogHighlights();

        foreach (var log in logs)
        {
            if (log.Location is null)
                continue;

            // Get the token's start location in utf8
            var location = log.Location.Value;
            var start = GetMappedIndex(location);

            // Get the token's string
            var highlightString = new StringBuilder();
            foreach (var token in log.Tokens)
            {
                highlightString.Append(token.Source);
            }

            // Find the start and length, using the string's length
            var tokenLength = Encoding.UTF8.GetByteCount($"{highlightString}");

            // Select the indictor
            editor.IndicatorCurrent = log.Severity switch
            {
                Severity.Error => ErrorIndicatorIndex,
                Severity.Warning => WarningIndicatorIndex,
                Severity.Message => MessageIndicatorIndex,
                _ => ErrorIndicatorIndex,
            };

            // Apply the indicator
            editor.IndicatorFillRange(start, tokenLength);

            // Don't add annotations for messages
            if (!MeetsThreshold(log.Severity))
                continue;

            // Apply annotation
            var annotationStyle = log.Severity switch
            {
                Severity.Error => ErrorAnnotationStyleIndex,
                Severity.Warning => WarningAnnotationStyleIndex,
                Severity.Message => MessageAnnotationStyleIndex,
                _ => ErrorAnnotationStyleIndex,
            };

            var line = location.Line;
            editor.AnnotationSetStyle(line, annotationStyle);
            editor.AnnotationSetText(line, log.Message);
        }

        editor.AnnotationVisible = AnnotationVisible.Boxed;
    }

    /// <summary>
    /// Clears formatting based on a log messages.
    /// </summary>
    public void ClearLogHighlights()
    {
        if (!TryGetEditor(out var editor))
            return;

        // Clear underlines
        for (int i = ErrorIndicatorIndex; i <= MessageIndicatorIndex; i++)
        {
            editor.IndicatorCurrent = i;
            editor.IndicatorClearRange(0, editor.Length);
        }

        // Clear annotations
        editor.AnnotationClearAll();
    }

    private void SetupIndicators()
    {
        if (!TryGetEditor(out var editor))
            return;

        if (ColorScheme is null)
            return;

        //editor.IndicSetStyle(ErrorIndicatorIndex, IndicatorStyle.Squiggle);
        editor.IndicSetStyle(ErrorIndicatorIndex, IndicatorStyle.SquigglePixmap);
        editor.IndicSetFore(ErrorIndicatorIndex, ToInt(ColorScheme.ErrorUnderlineColor));
        editor.IndicSetUnder(ErrorIndicatorIndex, true);

        //editor.IndicSetStyle(WarningIndicatorIndex, IndicatorStyle.Diagonal);
        editor.IndicSetStyle(WarningIndicatorIndex, IndicatorStyle.SquigglePixmap);
        editor.IndicSetFore(WarningIndicatorIndex, ToInt(ColorScheme.WarningUnderlineColor));
        editor.IndicSetUnder(WarningIndicatorIndex, true);

        editor.IndicSetStyle(MessageIndicatorIndex, IndicatorStyle.Plain);
        editor.IndicSetFore(MessageIndicatorIndex, ToInt(ColorScheme.MessageUnderlineColor));
        editor.IndicSetUnder(MessageIndicatorIndex, true);

        editor.IndicSetStyle(ExecutingLineIndicatorIndex, IndicatorStyle.StraightBox);
        editor.IndicSetFore(ExecutingLineIndicatorIndex, ToInt(Colors.Yellow));
        editor.IndicSetUnder(ExecutingLineIndicatorIndex, true);
        editor.IndicSetAlpha(ExecutingLineIndicatorIndex, (Alpha)0x80);
        editor.IndicSetOutlineAlpha(ExecutingLineIndicatorIndex, Alpha.Opaque);
    }

    private bool MeetsThreshold(Severity severity)
    {
        // The severity value is the threshold -1
        return (int)severity < (int)AnnotationThreshold;
    }
}
