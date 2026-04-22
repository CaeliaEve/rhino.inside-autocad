namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a circular arc entity from a Civil 3D Profile.
/// </summary>
/// <remarks>
/// Profile circular arcs are curved vertical segments with a constant radius.
/// </remarks>
public interface ICivilProfileCircularArc : ICivilProfileEntity
{
    /// <summary>
    /// Gets the radius of this circular arc in AutoCAD units.
    /// </summary>
    double Radius { get; }

    /// <summary>
    /// A boolean indicating whether this circular arc is a crest (convex) or sag (concave) curve.
    /// </summary>
    bool IsCrest { get; }

    /// <summary>
    /// The station along the profile where the high or low point of this circular arc occurs, in AutoCAD units.
    /// </summary>
    double HighLowPointStation { get; }

    /// <summary>
    ///  The elevation at the high or low point of this circular arc in AutoCAD units.
    /// </summary>
    double HighLowPointElevation { get; }
}
