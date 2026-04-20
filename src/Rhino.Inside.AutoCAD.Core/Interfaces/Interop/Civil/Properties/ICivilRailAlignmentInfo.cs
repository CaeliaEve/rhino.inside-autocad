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
    /// Gets the rail gauge value.
    /// </summary>
    /// <remarks>
    /// Returns 0 if this is not a rail alignment.
    /// </remarks>
    double Gauge { get; }

    /// <summary>
    /// Gets the CANT information for this rail alignment.
    /// </summary>
    /// <remarks>
    /// Returns empty CANT info if this is not a rail alignment or has no CANT data.
    /// </remarks>
    ICivilCANTInfo CANTInfo { get; }

    /// <summary>
    /// Creates a shallow copy of this rail alignment information.
    /// </summary>
    /// <returns>A new instance with the same values.</returns>
    ICivilRailAlignmentInfo ShallowClone();
}
