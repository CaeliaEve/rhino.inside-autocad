namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents an offset alignment region.
/// </summary>
public interface ICivilOffsetAlignmentRegion
{
    /// <summary>
    /// Gets the start station of this region.
    /// </summary>
    double StartStation { get; }

    /// <summary>
    /// Gets the end station of this region.
    /// </summary>
    double EndStation { get; }

    /// <summary>
    /// Gets the offset value for this region.
    /// </summary>
    double Offset { get; }
}