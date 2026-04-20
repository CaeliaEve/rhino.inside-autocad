using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a breakline definition from a Civil 3D TIN Surface.
/// </summary>
/// <remarks>
/// Surface breaklines in Civil 3D are lines that define surface edges where
/// elevation changes occur. This interface provides access to the breakline's
/// geometry, type, and name information.
/// </remarks>
public interface ICivilSurfaceBreakline
{
    /// <summary>
    /// Gets the breakline type (Standard, Wall, or NonDestructive).
    /// </summary>
    SurfaceBreaklineType BreaklineType { get; }

    /// <summary>
    /// Gets the breakline geometry as a Rhino curve.
    /// </summary>
    Curve Curve { get; }

    /// <summary>
    /// Gets the name of the breakline definition.
    /// </summary>
    string Name { get; }
}
