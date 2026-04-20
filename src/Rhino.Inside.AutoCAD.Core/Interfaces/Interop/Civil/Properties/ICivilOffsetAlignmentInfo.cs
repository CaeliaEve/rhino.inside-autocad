namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents offset alignment information for a Civil 3D Alignment.
/// </summary>
/// <remarks>
/// Provides information about offset alignments including the parent alignment,
/// nominal offset distance, and which side the offset is on.
/// </remarks>
public interface ICivilOffsetAlignmentInfo
{
    /// <summary>
    /// Gets a value indicating whether this alignment is an offset alignment.
    /// </summary>
    bool IsOffsetAlignment { get; }

    /// <summary>
    /// Gets the parent alignment as a NamedId.
    /// </summary>
    /// <remarks>
    /// Returns an empty NamedId if this is not an offset alignment.
    /// </remarks>
    INamedId ParentAlignment { get; }

    /// <summary>
    /// Gets the nominal offset distance from the parent alignment.
    /// </summary>
    /// <remarks>
    /// Returns 0 if this is not an offset alignment.
    /// </remarks>
    double NominalOffset { get; }

    /// <summary>
    /// Gets the side of the offset relative to the parent alignment.
    /// </summary>
    /// <remarks>
    /// Returns "Left" or "Right", or an empty string if not an offset alignment.
    /// </remarks>
    string OffsetSide { get; }

    /// <summary>
    /// Creates a shallow copy of this offset alignment information.
    /// </summary>
    /// <returns>A new instance with the same values.</returns>
    ICivilOffsetAlignmentInfo ShallowClone();
}
