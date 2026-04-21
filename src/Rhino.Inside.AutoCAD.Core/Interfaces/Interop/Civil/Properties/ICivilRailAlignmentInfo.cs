namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents rail alignment information for a Civil 3D Alignment.
/// </summary>
/// <remarks>
/// Provides rail-specific properties including gauge and CANT information
/// for rail alignments.
/// </remarks>
public interface ICivilRailAlignmentInfo
{
    /// <summary>
    /// Gets a value indicating whether this alignment is a rail alignment.
    /// </summary>
    bool IsRailAlignment { get; }

    /// <summary>
    /// Gets the rail Track Width value.
    /// </summary>
    /// <remarks>
    /// Returns 0 if this is not a rail alignment.
    /// </remarks>
    double TrackWidth { get; }

    /// <summary>
    /// Creates a shallow copy of this rail alignment information.
    /// </summary>
    /// <returns>A new instance with the same values.</returns>
    ICivilRailAlignmentInfo ShallowClone();
}
