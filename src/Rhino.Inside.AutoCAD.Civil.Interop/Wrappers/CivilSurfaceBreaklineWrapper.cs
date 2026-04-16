using Rhino.Geometry;
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
public class CivilSurfaceBreaklineWrapper : ICivilSurfaceBreakline
{
    /// <inheritdoc />
    public int BreaklineType { get; }

    /// <inheritdoc />
    public Curve Curve { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilSurfaceBreaklineWrapper"/>.
    /// </summary>
    /// <param name="breaklineType">
    /// The breakline type (0=Standard, 1=Wall, 2=Proximity, 3=NonDestructive).
    /// </param>
    /// <param name="curve">
    /// The breakline geometry as a Rhino curve.
    /// </param>
    /// <param name="name">
    /// The name of the breakline definition.
    /// </param>
    public CivilSurfaceBreaklineWrapper(int breaklineType, Curve curve, string name)
    {
        BreaklineType = breaklineType;
        Curve = curve;
        Name = name;
    }

    /// <summary>
    /// Creates a duplicate of this breakline wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilSurfaceBreaklineWrapper Duplicate()
    {
        var curveCopy = Curve.DuplicateCurve();
        return new CivilSurfaceBreaklineWrapper(BreaklineType, curveCopy, Name);
    }

    /// <summary>
    /// Gets a human-readable description of the breakline type.
    /// </summary>
    public string BreaklineTypeName => BreaklineType switch
    {
        0 => "Standard",
        1 => "Wall",
        2 => "Proximity",
        3 => "NonDestructive",
        _ => "Unknown"
    };

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Surface Breakline [{BreaklineTypeName}] \"{Name}\"";
    }
}
