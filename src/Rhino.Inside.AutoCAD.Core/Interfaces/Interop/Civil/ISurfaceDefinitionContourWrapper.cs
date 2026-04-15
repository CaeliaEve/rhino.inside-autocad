namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a contour definition from a Civil 3D TIN surface.
/// </summary>
/// <remarks>
/// Surface contour definitions specify elevation and interval data for contour lines
/// on a Civil 3D TIN surface. Used by Grasshopper components to extract and manipulate
/// contour data from Civil 3D surfaces. This represents the metadata about how contours
/// were added to the surface, not the actual contour geometry. To extract contour curves,
/// use the TinSurface.ExtractContours methods.
/// </remarks>
public interface ISurfaceDefinitionContourWrapper
{
    /// <summary>
    /// Gets the mid-ordinate distance used when adding contours to the surface.
    /// </summary>
    double MidOrdinateDistance { get; }

    /// <summary>
    /// Gets the maximum distance between points used when adding contours.
    /// </summary>
    double MaximumDistance { get; }

    /// <summary>
    /// Gets the weeding distance used when adding contours.
    /// </summary>
    double WeedingDistance { get; }

    /// <summary>
    /// Gets the weeding angle used when adding contours.
    /// </summary>
    double WeedingAngle { get; }

    /// <summary>
    /// Gets the description of this contour definition.
    /// </summary>
    string Description { get; }
}
