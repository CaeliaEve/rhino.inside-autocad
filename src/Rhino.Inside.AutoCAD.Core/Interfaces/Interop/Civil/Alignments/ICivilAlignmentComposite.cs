using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a composite sub-entity from a Civil 3D Alignment.
/// </summary>
/// <remarks>
/// Composite alignment entities represent complex geometry that may contain
/// multiple sub-components (such as spiral-curve-spiral groups). This interface
/// provides access to the combined geometry as a single polycurve.
/// </remarks>
public interface ICivilAlignmentComposite
{
    /// <summary>
    /// Gets the composite geometry as a Rhino polycurve.
    /// </summary>
    /// <remarks>
    /// The polycurve contains all component curves joined together.
    /// </remarks>
    PolyCurve Curve { get; }

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
    /// Gets the number of component segments in this composite.
    /// </summary>
    int ComponentCount { get; }

    /// <summary>
    /// Gets the index of this sub-entity within the alignment.
    /// </summary>
    int Index { get; }
}
