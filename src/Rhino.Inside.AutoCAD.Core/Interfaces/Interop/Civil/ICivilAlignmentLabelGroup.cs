namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a label group from a Civil 3D Alignment.
/// </summary>
/// <remarks>
/// Label groups are auto-generated labels that appear at regular intervals
/// along an alignment, such as station labels or geometry point labels.
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
}

/// <summary>
/// Interface for Civil 3D Alignment Cant label groups.
/// </summary>
public interface ICivilAlignmentCantLabelGroup : ICivilAlignmentLabelGroup
{
}

/// <summary>
/// Interface for Civil 3D Alignment Design Speed label groups.
/// </summary>
public interface ICivilAlignmentDesignSpeedLabelGroup : ICivilAlignmentLabelGroup
{
}

/// <summary>
/// Interface for Civil 3D Alignment Geometry Point label groups.
/// </summary>
public interface ICivilAlignmentGeometryPointLabelGroup : ICivilAlignmentLabelGroup
{
}

/// <summary>
/// Interface for Civil 3D Alignment Station Equation label groups.
/// </summary>
public interface ICivilAlignmentStationEquationLabelGroup : ICivilAlignmentLabelGroup
{
}

/// <summary>
/// Interface for Civil 3D Alignment Station label groups.
/// </summary>
public interface ICivilAlignmentStationLabelGroup : ICivilAlignmentLabelGroup
{
}

/// <summary>
/// Interface for Civil 3D Alignment Superelevation label groups.
/// </summary>
public interface ICivilAlignmentSuperelevationLabelGroup : ICivilAlignmentLabelGroup
{
}

/// <summary>
/// Interface for Civil 3D Alignment Vertical Geometry Point label groups.
/// </summary>
public interface ICivilAlignmentVerticalGeometryPointLabelGroup : ICivilAlignmentLabelGroup
{
}
