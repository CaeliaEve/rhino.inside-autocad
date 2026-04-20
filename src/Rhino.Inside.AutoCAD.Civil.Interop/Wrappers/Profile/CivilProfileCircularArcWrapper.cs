using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps a circular arc entity extracted from a Civil 3D Profile.
/// </summary>
/// <remarks>
/// This wrapper provides access to circular arc-specific properties like radius
/// and center point, in addition to the base entity properties.
/// </remarks>
public class CivilProfileCircularArcWrapper : CivilProfileEntityWrapper, ICivilProfileCircularArc
{
    private readonly ProfileCircular _profile;

    /// <inheritdoc />
    public double Radius { get; }

    /// <inheritdoc />
    public bool IsCrest { get; }

    /// <inheritdoc />
    public double HighLowPointStation { get; }

    /// <inheritdoc />
    public double HighLowPointElevation { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilProfileCircularArcWrapper"/>.
    /// </summary>
    /// <param name="profile">The civil3d profile.</param>
    /// <param name="entityIndex">The index of this entity in the profile's entity collection.</param>
    public CivilProfileCircularArcWrapper(
        ProfileCircular profile, int entityIndex)
        : base(profile, entityIndex)
    {
        _profile = profile;
        this.Radius = profile.Radius;

        this.IsCrest = profile.GradeIn > profile.GradeOut;

        this.HighLowPointStation = profile.HighLowPointStation;

        this.HighLowPointElevation = profile.HighLowPointElevation;
    }

    /// <summary>
    /// Creates a duplicate of this profile circular arc wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public override CivilProfileCircularArcWrapper ShallowClone()
    {
        return new CivilProfileCircularArcWrapper(_profile, this.EntityIndex);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Profile Circular Arc (Sta: {this.StartStation:F2} - {this.EndStation:F2}, Radius: {this.Radius:F2})";
    }
}
