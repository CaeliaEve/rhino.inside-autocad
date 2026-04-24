using Autodesk.AutoCAD.DatabaseServices;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Civil.Interop.Constants;
using Rhino.Inside.AutoCAD.Civil.Interop.Naming;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;
using RhinoMesh = Rhino.Geometry.Mesh;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that creates a Civil 3D TIN Surface from a Rhino Mesh.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class CreateCivilTINSurfaceComponent : RhinoInsideAutocad_CreateComponentBase
{
    private string _errorMessage = string.Empty;
    private const string _ghPrefix = CivilConstants.GhPrefix;

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
        if (ShouldSkipSolve()) return;

        AutocadDocument? autocadDocument = null;
        RhinoMesh? mesh = null;
        var surfaceName = string.Empty;
        GH_CivilSurfaceStyle? styleGoo = null;

        DA.GetData(0, ref autocadDocument);

        var document = this.GetDocumentOrDefault(autocadDocument);

        if (document is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No active AutoCAD document available");
            return;
        }

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

        var transactionManager = document.CreateTransactionManager();

        var result = transactionManager.PerformTask(() =>
        {
            try
            {
                var finalName = string.IsNullOrWhiteSpace(surfaceName)
                    ? AutoNamer.GenerateUniqueSurfaceName(transactionManager.AutocadDatabase, _ghPrefix)
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

        // Track the created object for potential replacement
        this.TrackCreatedObject(objectId, document);

        // Set outputs
        DA.SetData(0, new GH_CivilTinSurface(tinSurface));
        DA.SetData(1, createdName);
        DA.SetData(2, new GH_AutocadObjectId(new AutocadObjectIdWrapper(objectId)));
    }
}
