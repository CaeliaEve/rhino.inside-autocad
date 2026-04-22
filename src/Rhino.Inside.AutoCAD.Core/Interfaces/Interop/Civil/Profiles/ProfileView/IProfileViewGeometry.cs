using RhinoCurve = Rhino.Geometry.Curve;
using RhinoTextEntity = Rhino.Geometry.TextEntity;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represent the geometry of a Civil 3D Profile View converted to Rhino Types, including
/// both curves and text entities. This class serves as a container for all geometric
/// elements that make up the profile view, it is created from an exploded profile view. 
/// </summary>
public interface IProfileViewGeometry
{
    /// <summary>
    /// The list of curves that make up the profile view geometry.
    /// </summary>
    List<RhinoCurve> Curves { get; }

    /// <summary>
    /// The list of text entities that are part of the profile
    /// view geometry, such as labels and annotations.
    /// </summary>
    List<RhinoTextEntity> TextEntities { get; }
}