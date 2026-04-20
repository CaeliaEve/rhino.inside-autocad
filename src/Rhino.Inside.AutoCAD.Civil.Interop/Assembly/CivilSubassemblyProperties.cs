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
public record CivilSubassemblyProperties : ICivilSubassemblyProperties
{
    /// <summary>
    /// Constructs a new instance of <see cref="CivilSubassemblyProperties"/> by extracting
    /// data from a given <see cref="Subassembly"/>.
    /// </summary>
    public static CivilSubassemblyProperties CreateFromSubassembly(Subassembly subassembly)
    {
        return new CivilSubassemblyProperties()
        {
            Name = subassembly.Name,
            Description = subassembly.Description ?? string.Empty,
            Side = subassembly.Side.ToString(),
            Origin = subassembly.Origin.ToRhinoPoint3d(),
            Geometry = ExtractGeometry(subassembly),
        };
    }

    /// <inheritdoc />
    public string Name { get; init; } = string.Empty;

    /// <inheritdoc />
    public string Description { get; init; } = string.Empty;

    /// <inheritdoc />
    public string Side { get; init; } = string.Empty;

    /// <inheritdoc />
    public Point3d Origin { get; init; }

    /// <inheritdoc />
    public IReadOnlyList<Curve> Geometry { get; init; } = Array.Empty<Curve>();

    /// <summary>
    /// Initializes a new private empty instance of <see cref="CivilSubassemblyProperties"/>
    /// </summary>
    private CivilSubassemblyProperties()
    {
    }

    /// <summary>
    /// Creates a duplicate of this subassembly properties wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilSubassemblyProperties ShallowClone()
    {
        return new CivilSubassemblyProperties()
        {
            Name = this.Name,
            Description = this.Description,
            Side = this.Side,
            Origin = this.Origin,
            Geometry = this.Geometry.Select(c => c.DuplicateCurve()).ToList().AsReadOnly(),
        };
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Subassembly: {this.Name} (Side: {this.Side})";
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
}
