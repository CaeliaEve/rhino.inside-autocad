using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using Rhino.Inside.AutoCAD.Services;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Provides methods for creating TIN Volume Surfaces in Civil 3D.
/// </summary>
public static class TinVolumeSurfaceCreator
{

    /// <summary>
    /// Creates a TIN Volume Surface using IObjectId interfaces.
    /// </summary>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <param name="baseSurfaceIdRaw">The IObjectId of the base TIN surface.</param>
    /// <param name="comparisonSurfaceIdRaw">The IObjectId of the comparison TIN surface.</param>
    /// <param name="surfaceName">The name for the new volume surface.</param>
    /// <param name="styleId">Optional IObjectId of the surface style to apply.</param>
    /// <returns>The created TIN Volume Surface, or null if creation fails.</returns>
    public static TinVolumeSurface? Create(
        IAutocadTransactionManager transactionManager,
        IObjectId baseSurfaceIdRaw,
        IObjectId comparisonSurfaceIdRaw,
        string surfaceName,
        IObjectId? styleId = null)
    {
        try
        {

            var baseSurfaceId = baseSurfaceIdRaw.Unwrap();
            var comparisonSurfaceId = comparisonSurfaceIdRaw.Unwrap();

            // Create the volume surface
            // TinVolumeSurface.Create takes (name, baseSurfaceId, comparisonSurfaceId)
            var volumeSurfaceId = TinVolumeSurface.Create(
                surfaceName,
                baseSurfaceId,
                comparisonSurfaceId);

            var volumeSurface = volumeSurfaceId.GetObject(OpenMode.ForWrite) as TinVolumeSurface;

            if (volumeSurface == null)
            {
                LoggerService.Instance?.LogMessage("Failed to create TIN Volume Surface");
                return null;
            }

            // Apply style if provided
            if (styleId is not null && styleId.IsValid)
            {
                volumeSurface.StyleId = styleId.Unwrap();
            }

            // Rebuild the surface to compute volumes
            volumeSurface.Rebuild();

            return volumeSurface;
        }
        catch (System.Exception ex)
        {
            LoggerService.Instance?.LogError(ex,
                $"Civil TINVolumeSurface CreateVolumeSurface(surfaceName: {surfaceName})");
        }

        return null;
    }
}
