// Avishai Dernis 2025

using System;
using WinUIEditor;
using Zarem.IDE.Messages.Editor.Enums;

namespace Zarem.IDE.Controls.CodeEditor;

public partial class CodeEditor
{
    private void SetupKeybinds()
    {
        if (!TryGetEditor(out var editor))
            return;

        // Clear all built-in commands and restore only desired ones
        // This must be done to prevent undesired key captures.
        editor.ClearAllCmdKeys();

        // Tab
        editor.AssignCmdKey(KeyDef(Keys.Tab), (int)ScintillaMessage.Tab);
        editor.AssignCmdKey(KeyDef(Keys.Tab, KeyMod.Shift), (int)ScintillaMessage.BackTab);

        // New line
        editor.AssignCmdKey(KeyDef(Keys.Return), (int)ScintillaMessage.NewLine);
        editor.AssignCmdKey(KeyDef(Keys.Return, KeyMod.Shift), (int)ScintillaMessage.NewLine);

        // Backspace/Delete
        editor.AssignCmdKey(KeyDef(Keys.Back), (int)ScintillaMessage.DeleteBack);
        editor.AssignCmdKey(KeyDef(Keys.Back, KeyMod.Ctrl), (int)ScintillaMessage.DelWordLeft);
        editor.AssignCmdKey(KeyDef(Keys.Delete), (int)ScintillaMessage.Clear);
        editor.AssignCmdKey(KeyDef(Keys.Delete, KeyMod.Ctrl), (int)ScintillaMessage.DelWordRight);

        // Zoom
        editor.AssignCmdKey(KeyDef(Keys.Add, KeyMod.Ctrl), (int)ScintillaMessage.ZoomIn);
        editor.AssignCmdKey(KeyDef(Keys.Subtract, KeyMod.Ctrl), (int)ScintillaMessage.ZoomOut);

        // Line down keys
        editor.AssignCmdKey(KeyDef(Keys.Down), (int)ScintillaMessage.LineDown);
        editor.AssignCmdKey(KeyDef(Keys.Down, KeyMod.Shift), (int)ScintillaMessage.LineDownExtend);
        editor.AssignCmdKey(KeyDef(Keys.Down, KeyMod.Ctrl), (int)ScintillaMessage.LineScrollDown);
        editor.AssignCmdKey(KeyDef(Keys.Down, KeyMod.Shift | KeyMod.Alt), (int)ScintillaMessage.LineDownRectExtend); // TODO: Make command?

        // Line up keys
        editor.AssignCmdKey(KeyDef(Keys.Up), (int)ScintillaMessage.LineUp);
        editor.AssignCmdKey(KeyDef(Keys.Up, KeyMod.Shift), (int)ScintillaMessage.LineUpExtend);
        editor.AssignCmdKey(KeyDef(Keys.Up, KeyMod.Ctrl), (int)ScintillaMessage.LineScrollUp);
        editor.AssignCmdKey(KeyDef(Keys.Up, KeyMod.Shift | KeyMod.Alt), (int)ScintillaMessage.LineUpRectExtend); // TODO: Make command?

        // Line left keys
        editor.AssignCmdKey(KeyDef(Keys.Left), (int)ScintillaMessage.CharLeft);
        editor.AssignCmdKey(KeyDef(Keys.Left, KeyMod.Shift), (int)ScintillaMessage.CharLeftExtend);
        editor.AssignCmdKey(KeyDef(Keys.Left, KeyMod.Ctrl), (int)ScintillaMessage.WordLeft);
        editor.AssignCmdKey(KeyDef(Keys.Left, KeyMod.Ctrl | KeyMod.Shift), (int)ScintillaMessage.WordLeftExtend);

        // Line right keys
        editor.AssignCmdKey(KeyDef(Keys.Right), (int)ScintillaMessage.CharRight);
        editor.AssignCmdKey(KeyDef(Keys.Right, KeyMod.Shift), (int)ScintillaMessage.CharRightExtend);
        editor.AssignCmdKey(KeyDef(Keys.Right, KeyMod.Ctrl), (int)ScintillaMessage.WordRight);
        editor.AssignCmdKey(KeyDef(Keys.Right, KeyMod.Ctrl | KeyMod.Shift), (int)ScintillaMessage.WordRightExtend);
    }

    /// <summary>
    /// Applies an editor operation.
    /// </summary>
    public void ApplyOperation(EditorOperation operation)
    {
        // Get the editor
        var editor = ChildEditor?.Editor;
        if (editor is null)
            return;

        // Get action from child type
        var action = GetOperationAction(operation);

        // Select default behaviors if not overriden
        action ??= operation switch
        {
            EditorOperation.Undo => editor.Undo,
            EditorOperation.Redo => editor.Redo,
            EditorOperation.Cut => editor.SelectionEmpty ? editor.LineCut : editor.Cut,
            EditorOperation.Copy => editor.CopyAllowLine,
            EditorOperation.Paste => editor.Paste,
            EditorOperation.Duplicate => editor.SelectionEmpty ? editor.LineDuplicate : editor.SelectionDuplicate,
            EditorOperation.SelectAll => editor.SelectAll,
            EditorOperation.TransposeUp => () =>
            {
                editor.LineTranspose();
                editor.LineUp();
            },
            EditorOperation.TransposeDown => () =>
            {
                editor.LineDown();
                editor.LineTranspose();
            },
            EditorOperation.ExpandChildren => () => {
                var line = editor.LineFromPosition(editor.CurrentPos);
                line = editor.GetFoldParent(line);
                var level = editor.GetFoldLevel(line);
                editor.FoldChildren(line, FoldAction.Expand);
            },
            EditorOperation.CollapseChildren => () => {
                var line = editor.LineFromPosition(editor.CurrentPos);
                if ((editor.GetFoldLevel(line) & FoldLevel.HeaderFlag) is 0)
                {
                    line = editor.GetFoldParent(line);
                }
                var level = editor.GetFoldLevel(line);
                editor.FoldChildren(line, FoldAction.Contract);
            },
            EditorOperation.ExpandAll => () => {
                var line = editor.LineFromPosition(editor.CurrentPos);
                line = editor.GetFoldParent(line);
                var level = editor.GetFoldLevel(line);
                editor.FoldAll(FoldAction.Expand);
            },
            EditorOperation.CollapseAll => () => {
                var line = editor.LineFromPosition(editor.CurrentPos);
                line = editor.GetFoldParent(line);
                var level = editor.GetFoldLevel(line);
                editor.FoldAll(FoldAction.ContractEveryLevel);
            },
            _ => null,
        };

        if (action is null)
            return;

        // Perform the action
        action();
    }

    /// <summary>
    /// Gets the action for a given a <see cref="EditorOperation"/>.
    /// </summary>
    /// <param name="operation">The operation requested.</param>
    /// <returns>Null if default behavior should be used.</returns>
    protected virtual Action? GetOperationAction(EditorOperation operation) => null;

    private int KeyDef(char key, KeyMod mod = KeyMod.Norm)
    {
        return key + ((byte)mod << 16);
    }

    private int KeyDef(Keys key, KeyMod mod = KeyMod.Norm) => KeyDef((char)key, mod);
}
