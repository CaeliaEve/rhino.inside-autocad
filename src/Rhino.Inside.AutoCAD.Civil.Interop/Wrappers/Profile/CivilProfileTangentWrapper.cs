using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps a tangent (straight line) entity extracted from a Civil 3D Profile.
/// </summary>
/// <remarks>
/// This wrapper provides access to tangent-specific properties like grade,
/// in addition to the base entity properties.
/// </remarks>
public class CivilProfileTangentWrapper : CivilProfileEntityWrapper, ICivilProfileTangent
{
    private readonly ProfileTangent _tangent;

    /// <inheritdoc />
    public double Grade { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilProfileTangentWrapper"/>.
    /// </summary>
    /// <param name="tangent">The Civil3d tangent.</param>
    /// <param name="entityIndex">The index of this entity in the profile's entity collection.</param>
    public CivilProfileTangentWrapper(ProfileTangent tangent, int entityIndex)
        : base(tangent, entityIndex)
    {
        _tangent = tangent;
        this.Grade = tangent.Grade;
    }

    /// <summary>
    /// Creates a duplicate of this profile tangent wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public override CivilProfileTangentWrapper ShallowClone()
    {
        return new CivilProfileTangentWrapper(_tangent, this.EntityIndex);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Profile Tangent (Sta: {this.StartStation:F2} - {this.EndStation:F2}, Grade: {this.Grade:F2}%)";
    }
}
