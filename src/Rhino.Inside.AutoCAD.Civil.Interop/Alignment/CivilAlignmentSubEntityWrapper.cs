using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using RhinoCurve = Rhino.Geometry.Curve;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps the Civil 3D AlignmentSubEntity class, which represents a segment of an alignment (line, arc, spiral).
/// </summary>
public class CivilAlignmentSubEntityWrapper : AutocadWrapperBase<AlignmentSubEntity>, ICivilAlignmentSubEntity
{
    private readonly AlignmentSubEntity _entity;

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

    /// <summary>
    /// Initializes a new instance of <see cref="CivilAlignmentEntityWrapper"/>.
    /// </summary>
    public CivilAlignmentSubEntityWrapper(AlignmentSubEntity entity, int entityIndex) : base(entity)
    {
        _entity = entity;

        var (startStation, endStation, length) = this.GetEntityStationInfo(entity);

        this.EntityType = entity.SubEntityType.ToString();
        this.StartStation = startStation;
        this.EndStation = endStation;
        this.Length = length;
        this.EntityIndex = entityIndex;
    }

    /// <summary>
    /// Gets station information from an alignment entity.
    /// </summary>
    private (double StartStation, double EndStation, double Length) GetEntityStationInfo(
        AlignmentSubEntity entity)
    {
        // Each concrete type has its own station properties
        // Note: Civil 3D API reuses classes like AlignmentSCS for multiple entity types
        return entity switch
        {
            AlignmentSubEntityLine line => (line.StartStation, line.EndStation, line.Length),
            AlignmentSubEntityArc arc => (arc.StartStation, arc.EndStation, arc.Length),
            AlignmentSubEntitySpiral spiral => (spiral.StartStation, spiral.EndStation, spiral.Length),
            // Fallback: use curve length for unknown types
            _ => (0.0, 0, 0)
        };
    }

    /// <summary>
    /// Creates a duplicate of this alignment entity wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilAlignmentSubEntityWrapper ShallowClone()
    {

        return new CivilAlignmentSubEntityWrapper(_entity, this.EntityIndex);
    }

    /// <inheritdoc />
    public RhinoCurve ToRhinoCurve()
    {
        return _entity.ToRhinoCurve();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Sub Alignment Entity [{this.EntityType}] (Sta: {this.StartStation:F2} - {this.EndStation:F2}, Length: {this.Length:F2})";
    }
}
