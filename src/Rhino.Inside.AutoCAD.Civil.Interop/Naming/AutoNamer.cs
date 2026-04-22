using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Core.State;
using Rhino.Inside.AutoCAD.Interop;
using Surface = Autodesk.Civil.DatabaseServices.Surface;

namespace Rhino.Inside.AutoCAD.Civil.Interop.Naming;

/// <summary>
/// Provides methods for generating unique names for Civil 3D objects.
/// </summary>
public static class AutoNamer
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
    /// Generates a unique alignment name by checking existing alignments in the document.
    /// </summary>
    /// <param name="civilDoc">The Civil 3D document to check for existing alignment names.</param>
    /// <param name="prefix">The prefix to use for the alignment name.</param>
    /// <returns>A unique alignment name in the format "{prefix}Alignment_NNN".</returns>
    public static string GenerateUniqueAlignmentName(IAutocadTransactionManager autocadTransactionManager, string prefix)
    {
        var existingNames = GetAllAlignmentNames(autocadTransactionManager);

        var baseName = $"{prefix}Alignment";
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
        var existingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (ApplicationState.IsShuttingDown)
            return existingNames;

        var database = autocadDatabaseWrapper.Unwrap();
        var civilDoc = CivilApplication.ActiveDocument;

        if (civilDoc == null)
            return existingNames;

        try
        {
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
        }
        catch (System.Exception)
        {
            // Handle disposal during iteration - return whatever names we collected
        }

        return existingNames;
    }

    /// <summary>
    /// Gets all existing alignment names from the Civil 3D document.
    /// </summary>
    /// <param name="civilDoc">The Civil 3D document to check.</param>
    /// <returns>A set of all existing alignment names.</returns>
    private static HashSet<string> GetAllAlignmentNames(IAutocadTransactionManager transactionManager)
    {
        var database = transactionManager.AutocadDatabase.Unwrap();
        var civilDoc = CivilDocument.GetCivilDocument(database);

        var existingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (ApplicationState.IsShuttingDown)
            return existingNames;

        if (civilDoc == null)
            return existingNames;

        try
        {
            var alignmentIds = civilDoc.GetAlignmentIds();


            using var transaction = database.TransactionManager.StartTransaction();
            foreach (ObjectId alignmentId in alignmentIds)
            {
                if (alignmentId.IsValid && !alignmentId.IsNull && !alignmentId.IsErased)
                {
                    var alignment = transaction.GetObject(alignmentId, OpenMode.ForRead) as Alignment;
                    if (alignment != null)
                    {
                        existingNames.Add(alignment.Name);
                    }
                }
            }
            transaction.Commit();
        }
        catch (System.Exception)
        {
            // Handle disposal during iteration - return whatever names we collected
        }

        return existingNames;
    }
}
