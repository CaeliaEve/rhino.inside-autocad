namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// A wrapper around the Civil 3D TIN Surface object.
/// </summary>
public interface ICivilTinSurfaceWrapper
{
    /// <summary>
    /// The name of the TIN surface.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The Properties of the TIN surface, extracted from the Civil 3D database object.
    /// </summary>
    ICivilTinSurfaceProperties Properties { get; }

    /// <summary>
    /// Extracts both major and minor contours from a TIN surface using the surface's own settings.
    /// </summary>
    /// <param name="transaction">The current AutoCAD transaction.</param>
    /// <returns>A list of contour wrappers containing both major (Type=1) and minor (Type=2) contours.</returns>
    IReadOnlyList<ICivilSurfaceContour> GetContours(IAutocadTransactionManager transaction);

    /// <summary>
    /// Extracts all breaklines from a TIN surface.
    /// </summary>
    /// <param name="transaction">The current AutoCAD transaction.</param>
    /// <returns>A list of breakline wrappers containing the extracted breakline data.</returns>
    IReadOnlyList<ICivilSurfaceBreakline> GetBreaklines(IAutocadTransactionManager transaction);

    /// <summary>
    /// Extracts all boundaries from a TIN surface.
    /// </summary>
    /// <returns>A list of boundary wrappers containing the extracted boundary data.</returns>
    IReadOnlyList<ICivilSurfaceBoundary> GetBoundaries(IAutocadTransactionManager transaction);
}