// Avishai Dernis 2025

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WinUIEditor;
using Zarem.Assembler;
using Zarem.Assembler.Config;
using Zarem.Assembler.Handlers;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Logging.Enum;
using Zarem.Models.Instructions.Enums;

namespace Zarem.IDE.Controls.CodeEditor;

public partial class AssemblyEditor
{
    private const int ErrorIndicatorIndex = 8;
    private const int WarningIndicatorIndex = 9;
    private const int MessageIndicatorIndex = 10;

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
            if (!_locationMapper.TryGetValue(log.Location.Value.Index, out var utf8Location))
                continue;

            // Get the token's string
            var highlightString = new StringBuilder();
            foreach (var token in log.Tokens)
            {
                highlightString.Append(token.Source);
            }

            // Find the start and length, using the string's length
            var tokenLength = GetEncodingSize($"{highlightString}");
            var start = utf8Location.Index;

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

            var line = utf8Location.Line - 1;
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
        var editor = ChildEditor?.Editor;
        if (editor is null)
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

    private async Task RunAssemblerAsync()
    {
        // Skip assembling if disabled
        if (!RealTimeAssembly)
            return;

        // Run assembler and show errors
        try
        {
            var config = AssemblerConfig ?? new MIPSAssemblerConfig(MipsVersion.MipsIII);
            var result = await Zarembler.AssembleAsync(Text, "editor", new MIPSAssmblerHandler(config), config);
            ApplyLogHighlights(result.Logs);
            UpdateSymbols(result.Symbols);
        }
        catch (Exception)
        {
            // TODO: Notify exception occured
        }
    }

    private void SetupIndicators()
    {
        if (!TryGetEditor(out var editor))
            return;

        //editor.IndicSetStyle(ErrorIndicatorIndex, IndicatorStyle.Squiggle);
        editor.IndicSetStyle(ErrorIndicatorIndex, IndicatorStyle.SquigglePixmap);
        editor.IndicSetFore(ErrorIndicatorIndex, ToInt(SyntaxHighlightingTheme.ErrorUnderlineColor));
        editor.IndicSetUnder(ErrorIndicatorIndex, true);

        //editor.IndicSetStyle(WarningIndicatorIndex, IndicatorStyle.Diagonal);
        editor.IndicSetStyle(WarningIndicatorIndex, IndicatorStyle.SquigglePixmap);
        editor.IndicSetFore(WarningIndicatorIndex, ToInt(SyntaxHighlightingTheme.WarningUnderlineColor));
        editor.IndicSetUnder(WarningIndicatorIndex, true);

        editor.IndicSetStyle(MessageIndicatorIndex, IndicatorStyle.Plain);
        editor.IndicSetFore(MessageIndicatorIndex, ToInt(SyntaxHighlightingTheme.MessageUnderlineColor));
        editor.IndicSetUnder(MessageIndicatorIndex, true);
    }

    private bool MeetsThreshold(Severity severity)
    {
        // The severity value is the threshold -1
        return (int)severity < (int)AnnotationThreshold;
    }
}
