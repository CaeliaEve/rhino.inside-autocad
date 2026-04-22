namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents an offset alignment transition.
/// </summary>
public interface ICivilOffsetAlignmentTransition
{
    /// <summary>
    /// Gets the start station of this transition.
    /// </summary>
    double StartStation { get; }

    /// <summary>
    /// Gets the end station of this transition.
    /// </summary>
    double EndStation { get; }

    /// <summary>
    /// Gets the start offset value of this transition.
    /// </summary>
    double StartOffset { get; }

    /// <summary>
    /// Gets the end offset value of this transition.
    /// </summary>
    double EndOffset { get; }
}
