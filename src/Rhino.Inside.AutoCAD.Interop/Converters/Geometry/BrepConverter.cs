using Autodesk.AutoCAD.DatabaseServices;
using CadSolid3d = Autodesk.AutoCAD.DatabaseServices.Solid3d;
using RhinoBrep = Rhino.Geometry.Brep;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// Converts AutoCAD <see cref="CadSolid3d"/> objects into Rhino <see cref="RhinoBrep"/> geometry
/// via a file-based DXF export/import pipeline.
/// </summary>
/// <remarks>
/// The conversion process creates a temporary working <see cref="Database"/>, populates it with
/// a clone of the source solid, exports to a DXF file, then imports that file into a headless
/// <see cref="RhinoDoc"/> to extract Brep geometry.
/// <para>
/// Two population paths are supported depending on whether the solid is database-resident
/// (has a valid <see cref="ObjectId"/>) or transient. Transient solids receive database
/// defaults via <see cref="Entity.SetDatabaseDefaults"/> before export.
/// </para>
/// </remarks>
/// <seealso cref="RhinoBrep"/>
/// <seealso cref="CadSolid3d"/>
public class BrepConverter
{
    private static readonly string _tempDirectory = InteropConstants.TempDirectory;
    private static readonly string _dxfFilePath = InteropConstants.DxfFilePath;
    private const DwgVersion _dxfVersion = InteropConstants.DxfVersion;
    private const int _dxfPrecision = InteropConstants.DxfPrecision;

    /// <summary>
    /// Clones a database-resident solid into <paramref name="workingDatabase"/> without modifying
    /// the source database.
    /// </summary>
    /// <param name="solidId">
    /// The <see cref="ObjectId"/> of the solid to clone. Must belong to an open database.
    /// </param>
    /// <param name="workingDatabase">The temporary working database to receive the clone.</param>
    /// <remarks>
    /// Uses <see cref="Database.WblockCloneObjects"/> to perform a proper cross-database clone.
    /// This is the AutoCAD-recommended approach for transferring objects between databases because it:
    /// <list type="bullet">
    /// <item><description>Preserves entity properties (layer, color, linetype, etc.)</description></item>
    /// <item><description>Handles dependent objects (like layer table records) automatically</description></item>
    /// <item><description>Avoids <c>eWrongDatabase</c> errors that occur with manual <see cref="DBObject.Clone"/> approaches</description></item>
    /// </list>
    /// </remarks>
    private static void PopulateFromDatabaseResident(ObjectId solidId, Database workingDatabase)
    {
        var sourceDatabase = solidId.Database;
        var sourceIds = new ObjectIdCollection { solidId };

        ObjectId workingModelSpaceId;
        using (var transaction = workingDatabase.TransactionManager.StartTransaction())
        {
            var blockTable = (BlockTable)transaction.GetObject(workingDatabase.BlockTableId, OpenMode.ForRead);
            workingModelSpaceId = blockTable[BlockTableRecord.ModelSpace];
            transaction.Commit();
        }

        var mapping = new IdMapping();
        sourceDatabase.WblockCloneObjects(sourceIds, workingModelSpaceId, mapping,
            DuplicateRecordCloning.Replace, false);
    }

    /// <summary>
    /// Clones a transient solid into <paramref name="workingDb"/> and applies database defaults
    /// to the clone.
    /// </summary>
    /// <param name="solid">The transient solid to clone. Must not be database-resident.</param>
    /// <param name="workingDb">The temporary working database to receive the clone.</param>
    /// <remarks>
    /// Because transient solids have no owning database, defaults are applied explicitly so that
    /// the exported DXF contains well-formed entity properties.
    /// </remarks>
    private static void PopulateFromTransient(CadSolid3d solid, Database workingDb)
    {
        using (var clone = solid.Clone() as CadSolid3d)
        {
            AppendToModelSpace(clone, workingDb, applyDefaults: true);
        }
    }

    /// <summary>
    /// Appends <paramref name="solid"/> to the model space of <paramref name="database"/> within a
    /// committed transaction.
    /// </summary>
    /// <param name="solid">The solid to append. Ownership transfers to the database on commit.</param>
    /// <param name="database">The target working database.</param>
    /// <param name="applyDefaults">
    /// When <see langword="true"/>, calls <see cref="Entity.SetDatabaseDefaults"/> on the solid
    /// after it is added. Pass <see langword="true"/> for transient solids and
    /// <see langword="false"/> for clones of database-resident solids.
    /// </param>
    private static void AppendToModelSpace(CadSolid3d solid, Database database, bool applyDefaults)
    {
        using (var transaction = database.TransactionManager.StartTransaction())
        {
            var modelSpace = (BlockTableRecord)transaction.GetObject(
                SymbolUtilityServices.GetBlockModelSpaceId(database),
                OpenMode.ForWrite);

            modelSpace.AppendEntity(solid);
            transaction.AddNewlyCreatedDBObject(solid, true);

            if (applyDefaults)
                solid.SetDatabaseDefaults(database);

            transaction.Commit();
        }
    }

    /// <summary>
    /// Exports <paramref name="db"/> to the shared DXF file path defined in
    /// <see cref="InteropConstants"/>, creating the temp directory if it does not exist.
    /// </summary>
    /// <param name="db">The working database to export.</param>
    private static void ExportToDxf(Database db)
    {
        Directory.CreateDirectory(_tempDirectory);

        db.DxfOut(fileName: _dxfFilePath,
            precision: _dxfPrecision,
            version: _dxfVersion);
    }

    /// <summary>
    /// Imports the DXF file written by <see cref="ExportToDxf"/> into a headless
    /// <see cref="RhinoDoc"/> and returns a deep copy of every <see cref="RhinoBrep"/> found.
    /// </summary>
    /// <returns>
    /// An array of independent <see cref="RhinoBrep"/> copies. Returns an empty array if the
    /// document contains no Brep geometry.
    /// </returns>
    /// <remarks>
    /// The headless <see cref="RhinoDoc"/> is disposed before this method returns. Each returned
    /// Brep is a <see cref="RhinoBrep.DuplicateBrep"/> deep copy, safe to use after the document
    /// is closed.
    /// </remarks>
    private static RhinoBrep[] ImportAndExtractBreps()
    {
        using (var rhinoDoc = RhinoDoc.CreateHeadless(null))
        {
            rhinoDoc.Import(_dxfFilePath);

            var breps = new List<RhinoBrep>();

            foreach (var obj in rhinoDoc.Objects)
            {
                if (obj.Geometry is RhinoBrep brep)
                {
                    var deepCopy = brep.DuplicateBrep();
                    if (deepCopy is not null)
                        breps.Add(deepCopy);
                }
            }

            return breps.ToArray();
        }
    }

    /// <summary>
    /// Converts a <see cref="CadSolid3d"/> to an array of Rhino <see cref="RhinoBrep"/> objects.
    /// </summary>
    /// <param name="solid">
    /// The AutoCAD solid to convert. May be database-resident (valid
    /// <see cref="DBObject.ObjectId"/>) or transient.
    /// </param>
    /// <returns>
    /// An array of <see cref="RhinoBrep"/> instances extracted from the converted geometry, or
    /// <see cref="Array.Empty{T}"/> if conversion fails.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The method selects the appropriate population strategy based on
    /// <see cref="ObjectId.IsValid"/>: database-resident solids go through
    /// <see cref="PopulateFromDatabaseResident"/>; transient solids go through
    /// <see cref="PopulateFromTransient"/>.
    /// </para>
    /// <para>
    /// All exceptions are caught and suppressed; callers should treat an empty result as a
    /// conversion failure.
    /// </para>
    /// </remarks>
    /// <seealso cref="PopulateFromDatabaseResident"/>
    /// <seealso cref="PopulateFromTransient"/>
    public static RhinoBrep[] Convert(CadSolid3d solid)
    {
        try
        {
            using (var workingDb = new Database(buildDefaultDrawing: true, noDocument: true))
            {
                if (solid.ObjectId.IsValid)
                    PopulateFromDatabaseResident(solid.ObjectId, workingDb);
                else
                    PopulateFromTransient(solid, workingDb);

                ExportToDxf(workingDb);
            }

            return ImportAndExtractBreps();
        }
        catch
        {
            return [];
        }
    }
}