using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps breakline data extracted from a Civil 3D TIN Surface.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted breakline information.
/// Unlike other Civil 3D wrappers, this does not wrap a database object since
/// breaklines are extracted as temporary geometry from a TinSurface.
/// </remarks>
public class CivilSurfaceBreakline : ICivilSurfaceBreakline
{
    /// <inheritdoc />
    public SurfaceBreaklineType BreaklineType { get; }

    /// <inheritdoc />
    public Curve Curve { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilSurfaceBreakline"/>.
    /// </summary>
    /// <param name="breaklineType">
    /// The breakline type (Standard, Wall, or NonDestructive).
    /// </param>
    /// <param name="curve">
    /// The breakline geometry as a Rhino curve.
    /// </param>
    /// <param name="name">
    /// The name of the breakline definition.
    /// </param>
    public CivilSurfaceBreakline(SurfaceBreaklineType breaklineType, Curve curve, string name)
    {
        this.BreaklineType = breaklineType;
        this.Curve = curve;
        this.Name = name;
    }

    /// <summary>
    /// Creates a duplicate of this breakline wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilSurfaceBreakline Duplicate()
    {
        var curveCopy = this.Curve.DuplicateCurve();
        return new CivilSurfaceBreakline(this.BreaklineType, curveCopy, this.Name);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Surface Breakline [{this.BreaklineType}] \"{this.Name}\"";
    }
}
