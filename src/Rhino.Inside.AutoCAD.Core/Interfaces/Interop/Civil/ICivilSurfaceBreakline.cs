using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a breakline definition from a Civil 3D TIN Surface.
/// </summary>
/// <remarks>
/// Surface breaklines in Civil 3D are lines that define surface edges where
/// elevation changes occur. This interface provides access to the breakline's
/// geometry, type, and name information.
/// <para>
/// Breakline types are represented as integers:
/// <list type="bullet">
/// <item><description>0 = Standard breakline</description></item>
/// <item><description>1 = Wall breakline</description></item>
/// <item><description>2 = Proximity breakline</description></item>
/// <item><description>3 = Non-destructive breakline</description></item>
/// </list>
/// </para>
/// </remarks>
public interface ICivilSurfaceBreakline
{
    /// <summary>
    /// Gets the breakline type as an integer.
    /// </summary>
    /// <value>
    /// 0 = Standard, 1 = Wall, 2 = Proximity, 3 = NonDestructive
    /// </value>
    int BreaklineType { get; }

    /// <summary>
    /// Gets the breakline geometry as a Rhino curve.
    /// </summary>
    Curve Curve { get; }

    /// <summary>
    /// Gets the name of the breakline definition.
    /// </summary>
    string Name { get; }
}
