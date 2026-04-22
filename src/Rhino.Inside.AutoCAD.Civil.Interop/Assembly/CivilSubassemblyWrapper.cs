using Autodesk.Civil.DatabaseServices;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps properties extracted from a Civil 3D Subassembly.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted subassembly property information.
/// The data is captured at construction time from a <see cref="Subassembly"/>.
/// </remarks>
public class CivilSubassemblyWrapper : AutocadEntityWrapper, ICivilSubassembly
{
    private readonly Subassembly _subassembly;

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string Description { get; }

    /// <inheritdoc />
    public CivilSide Side { get; }

    /// <inheritdoc />
    public Point3d Origin { get; }

    /// <inheritdoc />
    public IReadOnlyList<Curve> Geometry { get; }

    /// <summary>
    /// Initializes a new private empty instance of <see cref="CivilSubassemblyWrapper"/>
    /// </summary>
    public CivilSubassemblyWrapper(Subassembly subassembly) : base(subassembly)
    {
        _subassembly = subassembly;
        this.Name = subassembly.Name;
        this.Description = subassembly.Description ?? string.Empty;
        this.Side = subassembly.Side.ToRhinoInsideSide();
        this.Origin = subassembly.Origin.ToRhinoPoint3d();
        this.Geometry = this.ExtractGeometry(subassembly);
    }

    /// <summary>
    /// Extracts geometry from a subassembly's links.
    /// </summary>
    private List<Curve> ExtractGeometry(Subassembly subassembly)
    {
        var curves = new List<Curve>();

        var links = subassembly.Links;

        foreach (var link in links)
        {
            var points = new List<Point3d>();

            foreach (var point in link.Points)
            {
                points.Add(new Point3d(
                    UnitConverter.ToRhinoLength(point.Offset),
                    0,
                    UnitConverter.ToRhinoLength(point.Elevation)));
            }

            if (points.Count >= 2)
            {
                curves.Add(new PolylineCurve(points));
            }
        }

        return curves;
    }

    /// <summary>
    /// Creates a duplicate of this subassembly properties wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilSubassemblyWrapper ShallowClone()
    {
        return new CivilSubassemblyWrapper(_subassembly);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Subassembly: {this.Name} (Side: {this.Side})";
    }
}
