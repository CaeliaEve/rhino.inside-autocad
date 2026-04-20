using Autodesk.Civil.DatabaseServices;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;

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
        HasReferencePoint = false;
        ReferencePoint = Point3d.Unset;
        ReferencePointStation = 0;
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
            HasReferencePoint = true;
            ReferencePoint = new Point3d(refPoint.X, refPoint.Y, refPoint.Z);
            ReferencePointStation = alignment.ReferencePointStation;
        }
        catch
        {
            HasReferencePoint = false;
            ReferencePoint = Point3d.Unset;
            ReferencePointStation = 0;
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
        if (!HasReferencePoint)
            return "No Reference Point";

        return $"Reference Station: {ReferencePointStation:F2} at ({ReferencePoint.X:F2}, {ReferencePoint.Y:F2}, {ReferencePoint.Z:F2})";
    }
}
