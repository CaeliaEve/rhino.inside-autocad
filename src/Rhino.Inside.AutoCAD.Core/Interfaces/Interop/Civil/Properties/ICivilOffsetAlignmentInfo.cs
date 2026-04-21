namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents offset alignment information for a Civil 3D Alignment.
/// </summary>
/// <remarks>
/// Wraps the OffsetAlignmentInfo from a Civil 3D Alignment, providing
/// offset distance, side, parent alignment, regions and transitions.
/// </remarks>
public interface ICivilOffsetAlignmentInfo
{
    /// <summary>
    /// Gets a value indicating whether this alignment is an offset alignment.
    /// </summary>
    bool IsOffsetAlignment { get; }

    /// <summary>
    /// Gets the nominal offset distance from the parent alignment.
    /// </summary>
    double NominalOffset { get; }

    /// <summary>
    /// Gets the side of the offset relative to the parent alignment.
    /// </summary>
    string Side { get; }

    /// <summary>
    /// Gets the parent alignment ObjectId.
    /// </summary>
    IObjectId ParentAlignmentId { get; }

    /// <summary>
    /// Gets the offset regions.
    /// </summary>
    IReadOnlyList<ICivilOffsetAlignmentRegion> Regions { get; }

    /// <summary>
    /// Creates a shallow copy of this offset alignment information.
    /// </summary>
    /// <returns>A new instance with the same values.</returns>
    ICivilOffsetAlignmentInfo ShallowClone();
}