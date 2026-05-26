using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// A service interface that provides methods for converting Civil 3D ProfileView properties
/// and geometry from there real world coordinate system (station and elevation) into the
/// coordinate system of the ProfileView.
/// </summary>
public interface IProfileViewCoordinateSystem
{
    /// <summary>
    /// Gets the insertion Plane (location) of the ProfileView.
    /// </summary>
    Plane Plane { get; }

    /// <summary>
    /// Gets the horizontal scale of the ProfileView.
    /// </summary>
    double HorizontalScale { get; }

    /// <summary>
    /// Gets the vertical scale of the ProfileView.
    /// </summary>
    double VerticalScale { get; }

    /// <summary>
    /// Gets the vertical exaggeration factor of the ProfileView.
    /// </summary>
    double VerticalExaggeration { get; }
}
