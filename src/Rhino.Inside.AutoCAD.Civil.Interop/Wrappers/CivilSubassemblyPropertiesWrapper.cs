using Autodesk.Civil.DatabaseServices;
using Rhino.Geometry;
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
public class CivilSubassemblyPropertiesWrapper : ICivilSubassemblyProperties
{
    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string Description { get; }

    /// <inheritdoc />
    public string Side { get; }

    /// <inheritdoc />
    public Point3d Origin { get; }

    /// <inheritdoc />
    public IReadOnlyList<Curve> Geometry { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilSubassemblyPropertiesWrapper"/>
    /// from a Civil 3D Subassembly.
    /// </summary>
    /// <param name="subassembly">The subassembly to extract properties from.</param>
    public CivilSubassemblyPropertiesWrapper(Subassembly subassembly)
    {
        this.Name = subassembly.Name;
        this.Description = subassembly.Description ?? string.Empty;
        this.Side = subassembly.Side.ToString();
        this.Origin = subassembly.Origin.ToRhinoPoint3d();
        this.Geometry = ExtractGeometry(subassembly);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilSubassemblyPropertiesWrapper"/>
    /// with explicit values.
    /// </summary>
    public CivilSubassemblyPropertiesWrapper(
        string name,
        string description,
        string side,
        Point3d origin,
        IReadOnlyList<Curve> geometry)
    {
        this.Name = name;
        this.Description = description;
        this.Side = side;
        this.Origin = origin;
        this.Geometry = geometry;
    }

    /// <summary>
    /// Extracts geometry from a subassembly's links.
    /// </summary>
    private static List<Curve> ExtractGeometry(Subassembly subassembly)
    {
        var curves = new List<Curve>();

        try
        {
            // Get the subassembly's calculated links which form the geometry
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
        }
        catch
        {
            // Return empty list if geometry extraction fails
        }

        return curves;
    }

    /// <summary>
    /// Creates a duplicate of this subassembly properties wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilSubassemblyPropertiesWrapper Duplicate()
    {
        return new CivilSubassemblyPropertiesWrapper(
            this.Name,
            this.Description,
            this.Side,
            this.Origin,
            this.Geometry.Select(c => c.DuplicateCurve()).ToList().AsReadOnly());
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Subassembly: {this.Name} (Side: {this.Side})";
    }
}
