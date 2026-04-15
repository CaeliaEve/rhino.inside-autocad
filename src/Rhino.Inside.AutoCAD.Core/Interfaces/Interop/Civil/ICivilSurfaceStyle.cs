namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a Civil 3D surface style.
/// </summary>
/// <remarks>
/// Surface styles in Civil 3D control the visual appearance of TIN surfaces,
/// including contour display settings, triangles, points, and watersheds.
/// This interface provides access to the style's identifying properties.
/// </remarks>
/// <seealso cref="INamedDbObject"/>
public interface ICivilSurfaceStyle : INamedDbObject
{
    // Inherits Name, Id, Type, IsValid, ShallowClone from INamedDbObject -> IDbObject
}
