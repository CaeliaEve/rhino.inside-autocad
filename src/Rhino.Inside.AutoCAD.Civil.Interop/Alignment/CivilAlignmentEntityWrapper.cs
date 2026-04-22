using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using RhinoCurve = Rhino.Geometry.Curve;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps an individual entity (segment) extracted from a Civil 3D Alignment.
/// </summary>
public class CivilAlignmentEntityWrapper : AutocadWrapperBase<AlignmentEntity>, ICivilAlignmentEntity
{
    private readonly AlignmentEntity _entity;

    /// <inheritdoc />
    public string EntityType { get; }

    /// <inheritdoc />
    public double StartStation { get; }

    /// <inheritdoc />
    public double EndStation { get; }

    /// <inheritdoc />
    public double Length { get; }

    /// <inheritdoc />
    public int EntityIndex { get; }

    /// <inheritdoc />
    public List<ICivilAlignmentSubEntity> SubEntities { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilAlignmentEntityWrapper"/>.
    /// </summary>
    public CivilAlignmentEntityWrapper(AlignmentEntity entity, int entityIndex) : base(entity)
    {
        _entity = entity;

        var (startStation, endStation, length) = this.GetEntityStationInfo(entity);

        var subEntities = new List<ICivilAlignmentSubEntity>();

        for (var index = 0; index < entity.SubEntityCount; index++)
        {
            var subEntity = entity[index];

            var wrappedSubEntity = new CivilAlignmentSubEntityWrapper(subEntity, index);

            subEntities.Add(wrappedSubEntity);
        }

        this.EntityType = entity.EntityType.ToString();
        this.StartStation = startStation;
        this.EndStation = endStation;
        this.Length = length;
        this.EntityIndex = entityIndex;
        this.SubEntities = subEntities;

    }

    /// <summary>
    /// Gets station information from an alignment entity.
    /// </summary>
    private (double StartStation, double EndStation, double Length) GetEntityStationInfo(
        AlignmentEntity entity)
    {
        // Each concrete type has its own station properties
        // Note: Civil 3D API reuses classes like AlignmentSCS for multiple entity types
        return entity switch
        {
            AlignmentLine line => (line.StartStation, line.EndStation, line.Length),
            AlignmentArc arc => (arc.StartStation, arc.EndStation, arc.Length),
            AlignmentSpiral spiral => (spiral.StartStation, spiral.EndStation, spiral.Length),
            AlignmentSCS scs => (scs.StartStation, scs.EndStation, scs.Length),
            AlignmentSTS sts => (sts.StartStation, sts.EndStation, sts.Length),
            AlignmentSSCSS sscss => (sscss.StartStation, sscss.EndStation, sscss.Length),
            AlignmentCRC crc => (crc.StartStation, crc.EndStation, crc.Length),
            // Fallback: use curve length for unknown types
            _ => (0.0, 0, 0)
        };
    }

    /// <summary>
    /// Creates a duplicate of this alignment entity wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilAlignmentEntityWrapper ShallowClone()
    {

        return new CivilAlignmentEntityWrapper(_entity, this.EntityIndex);
    }

    /// <inheritdoc />
    public RhinoCurve ToRhinoCurve()
    {
        return _entity.ToRhinoCurve();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Alignment Entity [{this.EntityType}] (Sta: {this.StartStation:F2} - {this.EndStation:F2}, Length: {this.Length:F2})";
    }
}
