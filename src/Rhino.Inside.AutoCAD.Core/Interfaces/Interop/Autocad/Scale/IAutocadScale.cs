namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a scale in AutoCAD.
/// </summary>
public interface IAutocadScale
{
    /// <summary>
    /// The scale factor in the X direction.
    /// </summary>
    double X { get; }

    /// <summary>
    /// The scale factor in the Y direction.
    /// </summary>
    double Y { get; }

    /// <summary>
    /// The scale factor in the Z direction.
    /// </summary>
    double Z { get; }

    /// <summary>
    /// Determines if this scale is equal to another scale by comparing their X, Y, and Z components.
    /// </summary>
    bool IsEqualTo(IAutocadScale other);
}