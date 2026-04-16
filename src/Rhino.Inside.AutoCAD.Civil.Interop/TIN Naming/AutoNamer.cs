using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using Surface = Autodesk.Civil.DatabaseServices.Surface;

namespace Rhino.Inside.AutoCAD.Civil.Interop.TIN_Naming;

public class AutoNamer
{
    /// <summary>
    /// Generates a unique surface name by checking existing surfaces in the database.
    /// </summary>
    /// <param name="autocadDatabaseWrapper">The database to check for existing surface names.</param>
    /// <param name="prefix">The prefix to use for the surface name.</param>
    /// <returns>A unique surface name in the format "{prefix}_TINSurface_NNN".</returns>
    public static string GenerateUniqueSurfaceName(IAutocadDatabase autocadDatabaseWrapper, string prefix)
    {
        var existingNames = GetAllSurfaceNames(autocadDatabaseWrapper);

        // Generate unique name
        var baseName = $"{prefix}_TINSurface";
        var counter = 1;
        string candidateName;

        do
        {
            candidateName = $"{baseName}_{counter:D3}";
            counter++;
        } while (existingNames.Contains(candidateName));

        return candidateName;
    }

    /// <summary>
    /// Generates a unique volume surface name by checking existing surfaces in the database.
    /// </summary>
    /// <param name="autocadDatabaseWrapper">The database to check for existing surface names.</param>
    /// <param name="prefix">The prefix to use for the volume surface name.</param>
    /// <returns>A unique volume surface name in the format "{prefix}_VolSurface_NNN".</returns>
    public static string GenerateUniqueVolumeSurfaceName(IAutocadDatabase autocadDatabaseWrapper, string prefix)
    {
        var existingNames = GetAllSurfaceNames(autocadDatabaseWrapper);

        // Generate unique name
        var baseName = $"{prefix}_VolSurface";
        var counter = 1;
        string candidateName;

        do
        {
            candidateName = $"{baseName}_{counter:D3}";
            counter++;
        } while (existingNames.Contains(candidateName));

        return candidateName;
    }

    /// <summary>
    /// Gets all existing surface names (TIN and Volume) from the database.
    /// </summary>
    /// <param name="autocadDatabaseWrapper">The database to check.</param>
    /// <returns>A set of all existing surface names.</returns>
    private static HashSet<string> GetAllSurfaceNames(IAutocadDatabase autocadDatabaseWrapper)
    {
        var database = autocadDatabaseWrapper.Unwrap();
        var civilDoc = CivilApplication.ActiveDocument;
        var existingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Collect existing surface names (both TIN and Volume surfaces)
        foreach (ObjectId surfaceId in civilDoc.GetSurfaceIds())
        {
            if (surfaceId.IsValid && !surfaceId.IsNull && !surfaceId.IsErased)
            {
                using var transaction = database.TransactionManager.StartTransaction();
                var surface = transaction.GetObject(surfaceId, OpenMode.ForRead) as Surface;
                if (surface != null)
                {
                    existingNames.Add(surface.Name);
                }

                transaction.Commit();
            }
        }

        return existingNames;
    }
}