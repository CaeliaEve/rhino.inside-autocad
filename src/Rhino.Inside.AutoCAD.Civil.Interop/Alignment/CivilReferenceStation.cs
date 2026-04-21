using Autodesk.Civil.DatabaseServices;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps reference station information extracted from a Civil 3D Alignment.
/// </summary>
public record CivilReferenceStation : ICivilReferenceStation
{
    /// <inheritdoc />
    public bool HasReferencePoint { get; }

    /// <inheritdoc />
    public Point3d ReferencePoint { get; }

    /// <inheritdoc />
    public double ReferencePointStation { get; }

    /// <summary>
    /// Gets an empty reference station with no reference point.
    /// </summary>
    public static CivilReferenceStation Empty { get; } = new();

    /// <summary>
    /// Initializes a new empty instance of <see cref="CivilReferenceStation"/>.
    /// </summary>
    private CivilReferenceStation()
    {
        this.HasReferencePoint = false;
        this.ReferencePoint = Point3d.Unset;
        this.ReferencePointStation = 0;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilReferenceStation"/> from an Alignment.
    /// </summary>
    /// <param name="alignment">The Civil 3D alignment to extract reference station from.</param>
    public CivilReferenceStation(Alignment alignment)
    {
        try
        {
            var refPoint = alignment.ReferencePoint;
            this.HasReferencePoint = true;
            this.ReferencePoint = refPoint.ToRhinoPoint3d();
            this.ReferencePointStation = alignment.ReferencePointStation;
        }
        catch
        {
            this.HasReferencePoint = false;
            this.ReferencePoint = Point3d.Unset;
            this.ReferencePointStation = 0;
        }
    }

    /// <inheritdoc />
    public ICivilReferenceStation ShallowClone()
    {
        return this with { };
    }

    /// <summary>
    /// Returns a string representation of this reference station.
    /// </summary>
    public override string ToString()
    {
        if (!this.HasReferencePoint)
            return "No Reference Point";

        return $"Reference Station: {this.ReferencePointStation:F2} at ({this.ReferencePoint.X:F2}, {this.ReferencePoint.Y:F2}, {this.ReferencePoint.Z:F2})";
    }
}
