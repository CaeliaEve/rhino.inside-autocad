namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents connected alignment information for a Civil 3D Alignment.
/// </summary>
/// <remarks>
/// Provides information about parent and child alignment relationships,
/// used for intersecting and connected alignment types.
/// </remarks>
public interface ICivilConnectedAlignmentInfo
{
    /// <summary>
    /// Gets the parent alignment as a NamedId.
    /// </summary>
    /// <remarks>
    /// Returns an empty NamedId if this alignment has no parent.
    /// </remarks>
    INamedId ParentAlignment { get; }

    /// <summary>
    /// Gets the collection of child alignments as NamedIds.
    /// </summary>
    IReadOnlyList<INamedId> ChildAlignments { get; }

    /// <summary>
    /// Gets a value indicating whether this alignment is connected to other alignments.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Creates a shallow copy of this connected alignment information.
    /// </summary>
    /// <returns>A new instance with the same values.</returns>
    ICivilConnectedAlignmentInfo ShallowClone();
}
