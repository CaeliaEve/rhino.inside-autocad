using RhinoCurve = Rhino.Geometry.Curve;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// A wrapper for a Civil 3D Alignment entity, providing access to its properties,
/// geometry, entities, and labels in Rhino format.
/// </summary>
/// <remarks>
/// This interface provides comprehensive access to alignment data including
/// the centerline curve, individual geometric entities (lines, arcs, spirals),
/// label groups, and individual feature labels.
/// </remarks>
public interface ICivilAlignment : IEntity
{
    /// <summary>
    /// Gets the ObjectId of the style applied to this alignment.
    /// </summary>
    IObjectId StyleId { get; }

    /// <summary>
    /// Gets the properties of this alignment, including name, description,
    /// station information, and related metadata.
    /// </summary>
    ICivilAlignmentProperties Properties { get; }

    /// <summary>
    /// Gets the individual geometric entities (Lines, Arcs, Spirals) that make up
    /// the alignment geometry.
    /// </summary>
    List<ICivilAlignmentEntity> Entities { get; }

    /// <summary>
    /// Gets the alignment centerline as a Rhino curve (typically a PolyCurve
    /// composed of the individual entities).
    /// </summary>
    RhinoCurve? CenterlineCurve { get; }

    /// <summary>
    /// Gets the auto-generated label groups from the alignment, such as
    /// station labels or geometry point labels.
    /// </summary>
    List<ICivilAlignmentLabelGroup> LabelGroups { get; }

    /// <summary>
    /// Gets the individual feature labels from the alignment, including
    /// curve labels, spiral labels, tangent labels, and PI labels.
    /// </summary>
    List<ICivilFeatureLabel> Labels { get; }
}
