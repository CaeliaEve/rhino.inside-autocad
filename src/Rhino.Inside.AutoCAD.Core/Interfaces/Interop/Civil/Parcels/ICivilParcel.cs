using RhinoCurve = Rhino.Geometry.Curve;
using RhinoPoint = Rhino.Geometry.Point3d;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// A wrapper for a Civil 3D Parcel entity, providing access to its properties and geometry in Rhino format.
/// </summary>
public interface ICivilParcel : IEntity
{
    /// <summary>
    /// The properties of this parcel, extracted from its base curve and other relevant data.
    /// </summary>
    ICivilParcelProperties Properties { get; }

    /// <summary>
    /// The Rhino curve representing the boundary of this parcel, converted from the Civil 3D base curve.
    /// </summary>
    RhinoCurve BoundaryCurve { get; }

    /// <summary>
    /// The centroid of this parcel, calculated from the base curve geometry. This provides a convenient
    /// reference point for the parcel in Rhino Units.
    /// </summary>
    RhinoPoint Centroid { get; }

    /// <summary>
    /// The individual segments that make up the parcel boundary. For a AutoCAD polyline base curve, this will
    /// be a list of line and arc segments otherwise it will contain a single segment representing
    /// the whole curve.
    /// </summary>
    List<ICivilParcelSegment> Segments { get; }
}