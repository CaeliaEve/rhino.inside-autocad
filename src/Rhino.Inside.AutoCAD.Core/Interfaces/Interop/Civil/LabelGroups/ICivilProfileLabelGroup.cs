namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a label group from a Civil 3D Profile.
/// </summary>
/// <remarks>
/// Label groups are auto-generated labels that appear at regular intervals
/// along a profile, such as station-elevation labels or grade break labels.
/// </remarks>
public interface ICivilProfileLabelGroup
{
    /// <summary>
    /// Gets the type of this label group.
    /// </summary>
    /// <value>
    /// The type name of the label group class (e.g., "ProfileLabelGroup").
    /// </value>
    string LabelGroupType { get; }

    /// <summary>
    /// Gets the name of the label style applied to this group.
    /// </summary>
    string StyleName { get; }

    /// <summary>
    /// Gets the number of sub-entity labels in this group.
    /// </summary>
    int LabelCount { get; }

    /// <summary>
    /// Gets the start station of the label range.
    /// </summary>
    double RangeStart { get; }

    /// <summary>
    /// Gets the end station of the label range.
    /// </summary>
    double RangeEnd { get; }

    /// <summary>
    /// Gets a value indicating whether the start of the range is derived from the profile feature.
    /// </summary>
    bool RangeStartFromFeature { get; }

    /// <summary>
    /// Gets a value indicating whether the end of the range is derived from the profile feature.
    /// </summary>
    bool RangeEndFromFeature { get; }

    /// <summary>
    /// Gets a value indicating whether the label group is visible.
    /// </summary>
    bool IsVisible { get; }
}
