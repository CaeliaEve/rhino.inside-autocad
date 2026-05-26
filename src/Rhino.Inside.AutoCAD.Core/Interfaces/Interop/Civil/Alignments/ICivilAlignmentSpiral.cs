using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a spiral sub-entity from a Civil 3D Alignment.
/// </summary>
/// <remarks>
/// Alignment spirals are transition curves (clothoids) that provide a gradual
/// change in curvature between alignment elements. This interface provides
/// access to the spiral's geometry and defining parameters.
/// </remarks>
public interface ICivilAlignmentSpiral
{
    /// <summary>
    /// Gets the spiral geometry as a Rhino curve.
    /// </summary>
    /// <remarks>
    /// Civil 3D spirals are converted to NURBS curves for representation in Rhino.
    /// </remarks>
    Curve Curve { get; }

    /// <summary>
    /// Gets the starting station of this sub-entity along the alignment.
    /// </summary>
    double StartStation { get; }

    /// <summary>
    /// Gets the ending station of this sub-entity along the alignment.
    /// </summary>
    double EndStation { get; }

    /// <summary>
    /// Gets the length of this sub-entity.
    /// </summary>
    double Length { get; }

    /// <summary>
    /// Gets the radius at the start of the spiral.
    /// </summary>
    /// <remarks>
    /// A value of infinity (double.PositiveInfinity) indicates a tangent connection.
    /// </remarks>
    double RadiusIn { get; }

    /// <summary>
    /// Gets the radius at the end of the spiral.
    /// </summary>
    /// <remarks>
    /// A value of infinity (double.PositiveInfinity) indicates a tangent connection.
    /// </remarks>
    double RadiusOut { get; }

    /// <summary>
    /// Gets the spiral definition type name (e.g., "Clothoid", "Bloss", etc.).
    /// </summary>
    string SpiralType { get; }

    /// <summary>
    /// Gets a value indicating whether the spiral curves clockwise.
    /// </summary>
    bool IsClockwise { get; }

    /// <summary>
    /// Gets the index of this sub-entity within the alignment.
    /// </summary>
    int Index { get; }
}
