using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;
using RhinoMesh = Rhino.Geometry.Mesh;
using TinSurface = Autodesk.Civil.DatabaseServices.TinSurface;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that creates a Civil 3D TIN Surface from a Rhino Mesh.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class CreateCivilTINSurfaceComponent : RhinoInsideAutocad_ComponentBase
{
    private string _errorMessage = string.Empty;

    /// <inheritdoc />
    public override Guid ComponentGuid => new("B4C8D9E2-6F3A-4B7C-8D5E-2A9F1C4B3D6E");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CreateCivilTINSurface;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCivilTINSurfaceComponent"/> class.
    /// </summary>
    public CreateCivilTINSurfaceComponent()
        : base("Create Civil3d TIN Surface", "CVL-CreateSrf",
            "Creates a Civil 3D TIN surface from a Rhino Mesh",
            "Civil3d", "Surfaces")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadDocument(GH_ParamAccess.item), "Document",
            "Doc", "An AutoCAD Document. If not provided, the active document will be used.", GH_ParamAccess.item);
        pManager[0].Optional = true;

        pManager.AddMeshParameter("Mesh", "M",
            "The Rhino Mesh to convert to a TIN Surface.", GH_ParamAccess.item);

        pManager.AddTextParameter("Name", "N",
            "The name for the TIN Surface. If not provided, a unique name will be auto-generated (e.g., GH_TINSurface_001).",
            GH_ParamAccess.item);
        pManager[2].Optional = true;

        pManager.AddParameter(new Param_CivilSurfaceStyle(GH_ParamAccess.item), "Style", "S",
            "The surface style to apply. Can be a style name (string) or a style object. Uses Civil 3D default if not provided.",
            GH_ParamAccess.item);
        pManager[3].Optional = true;
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilTinSurface(), "Surface", "Srf",
            "The created TIN Surface.", GH_ParamAccess.item);

        pManager.AddTextParameter("Name", "N",
            "The name of the created surface (useful when auto-generated).", GH_ParamAccess.item);

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "Id", "Id",
            "The ObjectId of the created surface.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        AutocadDocument? autocadDocument = null;
        RhinoMesh? mesh = null;
        var surfaceName = string.Empty;
        GH_CivilSurfaceStyle? styleGoo = null;

        DA.GetData(0, ref autocadDocument);

        if (autocadDocument is null)
        {
            var activeDoc = RhinoInsideAutoCadExtension.Application?.RhinoInsideManager?
                .AutoCadInstance?.ActiveDocument;
            if (activeDoc is null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No active AutoCAD document available");
                return;
            }
            autocadDocument = activeDoc as AutocadDocument;
        }

        if (autocadDocument is null)
            return;

        if (!DA.GetData(1, ref mesh) || mesh is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No mesh provided");
            return;
        }

        // Validate mesh
        if (mesh.Vertices.Count < 3)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Mesh must have at least 3 vertices");
            return;
        }

        DA.GetData(2, ref surfaceName);
        DA.GetData(3, ref styleGoo);

        _errorMessage = string.Empty;

        var result = autocadDocument.Transaction((transactionManager) =>
        {
            try
            {
                var database = transactionManager.Database.Unwrap();

                // Generate unique name if not provided
                var finalName = string.IsNullOrWhiteSpace(surfaceName)
                    ? GenerateUniqueSurfaceName(database)
                    : surfaceName;

                // Get style ObjectId if provided
                ObjectId? styleId = null;
                if (styleGoo?.Value != null)
                {
                    styleId = styleGoo.Value.Id.Unwrap();
                }

                // Create the TIN surface
                var surface = mesh.ToTinSurface(transactionManager, finalName, styleId);

                if (surface == null)
                {
                    _errorMessage = "Failed to create TIN Surface. The surface name may already exist.";
                    return (null, string.Empty, ObjectId.Null);
                }

                return (surface, finalName, surface.Id);
            }
            catch (Autodesk.Civil.CivilException ex)
            {
                _errorMessage = $"Civil 3D error: {ex.Message}";
                return (null, string.Empty, ObjectId.Null);
            }
            catch (System.Exception ex)
            {
                _errorMessage = $"Failed to create TIN Surface: {ex.Message}";
                return (null, string.Empty, ObjectId.Null);
            }
        });

        if (!string.IsNullOrEmpty(_errorMessage))
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, _errorMessage);
            return;
        }

        var (tinSurface, createdName, objectId) = result;

        if (tinSurface == null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to create TIN Surface");
            return;
        }

        // Set outputs
        DA.SetData(0, new GH_CivilTinSurface(tinSurface));
        DA.SetData(1, createdName);
        DA.SetData(2, new GH_AutocadObjectId(new AutocadObjectIdWrapper(objectId)));
    }

    /// <summary>
    /// Generates a unique surface name by checking existing surfaces in the database.
    /// </summary>
    /// <param name="database">The database to check for existing surface names.</param>
    /// <returns>A unique surface name in the format "GH_TINSurface_NNN".</returns>
    private static string GenerateUniqueSurfaceName(Database database)
    {
        var civilDoc = CivilApplication.ActiveDocument;
        var existingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Collect existing surface names
        foreach (ObjectId surfaceId in civilDoc.GetSurfaceIds())
        {
            if (surfaceId.IsValid && !surfaceId.IsNull && !surfaceId.IsErased)
            {
                using var transaction = database.TransactionManager.StartTransaction();
                var surface = transaction.GetObject(surfaceId, OpenMode.ForRead) as TinSurface;
                if (surface != null)
                {
                    existingNames.Add(surface.Name);
                }
                transaction.Commit();
            }
        }

        // Generate unique name
        const string baseName = "GH_TINSurface";
        var counter = 1;
        string candidateName;

        do
        {
            candidateName = $"{baseName}_{counter:D3}";
            counter++;
        }
        while (existingNames.Contains(candidateName));

        return candidateName;
    }
}
