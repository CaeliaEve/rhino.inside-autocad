using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

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
    public double StartStation { get; }

    /// <inheritdoc />
    public double EndStation { get; }

    /// <inheritdoc />
    public double StartElevation { get; }

    /// <inheritdoc />
    public double EndElevation { get; }

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

        this.StartStation = entity.StartStation;

        this.EndStation = entity.EndStation;

        this.StartElevation = entity.StartElevation;

        this.EndElevation = entity.EndElevation;

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
    public override string ToString()
    {
        return $"Profile Entity [{this.EntityType}] (Sta: {this.StartStation:F2} - {this.EndStation:F2}, Elev: {this.StartElevation:F2} - {this.EndElevation:F2})";
    }
}
