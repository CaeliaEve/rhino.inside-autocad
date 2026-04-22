namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a label group from a Civil 3D Alignment.
/// </summary>
/// <remarks>
/// Label groups are auto-generated labels that appear at regular intervals
/// along an alignment, such as station labels or geometry point labels.
/// All alignment label group types (Station, Cant, DesignSpeed, GeometryPoint,
/// StationEquation, Superelevation, VerticalGeometryPoint) share these same properties.
/// </remarks>
public interface ICivilAlignmentLabelGroup
{
    /// <summary>
    /// Gets the type of this label group.
    /// </summary>
    /// <value>
    /// The type name of the label group class (e.g., "AlignmentStationLabelGroup").
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
    /// Gets a value indicating whether the start of the range is derived from the alignment feature.
    /// </summary>
    bool RangeStartFromFeature { get; }

    /// <summary>
    /// Gets a value indicating whether the end of the range is derived from the alignment feature.
    /// </summary>
    bool RangeEndFromFeature { get; }

    /// <summary>
    /// Gets a value indicating whether the label group is visible.
    /// </summary>
    bool IsVisible { get; }
}
