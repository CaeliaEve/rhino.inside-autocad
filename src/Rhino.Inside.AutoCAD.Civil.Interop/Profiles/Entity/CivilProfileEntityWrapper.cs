using Autodesk.Civil.DatabaseServices;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using RhinoLine = Rhino.Geometry.Line;
using RhinoLineCurve = Rhino.Geometry.LineCurve;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps an individual entity (segment) extracted from a Civil 3D Profile.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted profile entity information.
/// Unlike other Civil 3D wrappers, this does not wrap a database object since
/// profile entities are extracted as temporary geometry from a Profile.
/// </remarks>
public class CivilProfileEntityWrapper : AutocadWrapperBase<ProfileEntity>, ICivilProfileEntity
{
    private readonly ProfileEntity _entity;

    /// <inheritdoc />
    public string EntityType { get; }

    /// <inheritdoc />
    public ICivilStationPoint Start { get; }

    /// <inheritdoc />
    public ICivilStationPoint End { get; }

    /// <inheritdoc />
    public double Length { get; }

    /// <inheritdoc />
    public int EntityIndex { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilProfileEntityWrapper"/>.
    /// </summary>
    /// <param name="entity">The Civil entity</param>
    /// <param name="entityIndex">The index of this entity in the profile's entity collection.</param>
    public CivilProfileEntityWrapper(ProfileEntity entity, int entityIndex) : base(entity)
    {
        _entity = entity;

        this.EntityType = entity.EntityType.ToString();

        this.Start = new CivilStationPoint(entity.StartStation, entity.StartElevation);

        this.End = new CivilStationPoint(entity.EndStation, entity.EndElevation);

        this.Length = entity.Length;

        this.EntityIndex = entityIndex;
    }

    /// <summary>
    /// Creates a duplicate of this profile entity wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public virtual CivilProfileEntityWrapper ShallowClone()
    {
        return new CivilProfileEntityWrapper(_entity, this.EntityIndex);
    }

    /// <inheritdoc />
    public virtual Curve ToRhinoCurve()
    {
        var startPoint = this.Start.ToRhinoPoint3d();
        var endPoint = this.End.ToRhinoPoint3d();
        return new RhinoLineCurve(new RhinoLine(startPoint, endPoint));
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Profile Entity [{this.EntityType}] (Start:[{this.Start}] - End:[{this.End}])";
    }
}
