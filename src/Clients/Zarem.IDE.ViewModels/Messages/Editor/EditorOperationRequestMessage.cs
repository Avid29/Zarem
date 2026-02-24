// Avishai Dernis 2025

using Zarem.Messages.Editor.Enums;

namespace Zarem.Messages.Editor;

/// <summary>
/// A message sent to request an editor operation.
/// </summary>
public class EditorOperationRequestMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EditorOperationRequestMessage"/> class.
    /// </summary>
    public EditorOperationRequestMessage(EditorOperation operation)
    {
        Operation = operation;
    }

    /// <summary>
    /// Gets the operation to apply.
    /// </summary>
    public EditorOperation Operation { get; }
}
