namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// The base of <see cref="ICivilAlignmentEntity"/>  and <see
/// cref="ICivilAlignmentSubEntity"/> entities.
/// </summary>

public interface ICivilAlignmentEntityBase
{
    /// <summary>
    /// Gets the type of entity as a string.
    /// </summary>
    /// <value>
    /// Common values: "Line", "Arc", "Spiral", "SpiralCurveSpiral",
    /// "SpiralLineSpiral", "SpiralLine", "LineSpiral", etc.
    /// </value>
    string EntityType { get; }

    /// <summary>
    /// Gets the starting station of this entity along the alignment.
    /// </summary>
    double StartStation { get; }

    /// <summary>
    /// Gets the ending station of this entity along the alignment.
    /// </summary>
    double EndStation { get; }

    /// <summary>
    /// Gets the length of this entity.
    /// </summary>
    double Length { get; }

    /// <summary>
    /// Gets the index of this entity within the alignment's entity collection.
    /// </summary>
    int EntityIndex { get; }

    /// <summary>
    /// Returns a Rhino.Geometry curve representing the geometry of this alignment entity.
    /// </summary>
    /// <returns></returns>
    Rhino.Geometry.Curve ToRhinoCurve();
}