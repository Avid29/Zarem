// Avishai Dernis 2025

using Zarem.Assembler.Tokenization.Models;

namespace Zarem.Messages.Navigation;

/// <summary>
/// A message sent requesting to go to a token in a file.
/// </summary>
public class NavigateToTokenRequestMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NavigateToTokenRequestMessage"/> class.
    /// </summary>
    public NavigateToTokenRequestMessage(Token token)
    {
        Target = token;
    }

    /// <summary>
    /// Gets the target token to visit.
    /// </summary>
    public Token Target { get; }
}
