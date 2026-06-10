using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Civil.Interop.Constants;
using Rhino.Inside.AutoCAD.Civil.Interop.Naming;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that creates a Civil 3D TIN Volume Surface from two TIN Surfaces.
/// </summary>
[ComponentVersion(introduced: "1.1.19")]
public class CreateCivilTINVolumeSurfaceComponent : RhinoInsideAutocad_CreateComponentBase
{
    private string _errorMessage = string.Empty;
    private const string _ghPrefix = CivilConstants.GhVolumeSurfacePrefix;

    /// <inheritdoc />
    public override Guid ComponentGuid => new("C8D9E2F1-7A5B-4C3D-8E6F-2B1A9C4D5E7F");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CreateCivilTINVolumeSurfaceComponent;

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.secondary;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCivilTINVolumeSurfaceComponent"/> class.
    /// </summary>
    public CreateCivilTINVolumeSurfaceComponent()
        : base("Create Civil3d TIN Volume Surface", "CVL-CreateVolSrf",
            "Creates a Civil 3D TIN Volume surface from two TIN surfaces to calculate cut/fill volumes",
            "Civil3d", "Surfaces")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadDocument(GH_ParamAccess.item), "Document",
            "Doc", "An AutoCAD Document. If not provided, the active document will be used.", GH_ParamAccess.item);
        pManager[0].Optional = true;

        pManager.AddParameter(new Param_CivilTinSurface(), "Base Surface", "BaseSrf",
            "The base TIN surface for volume calculation.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_CivilTinSurface(), "Comparison Surface", "CompSrf",
            "The comparison TIN surface for volume calculation.", GH_ParamAccess.item);

        pManager.AddTextParameter("Name", "N",
            "The name for the Volume Surface. If not provided, a unique name will be auto-generated.",
            GH_ParamAccess.item);
        pManager[3].Optional = true;

        pManager.AddParameter(new Param_CivilSurfaceStyle(GH_ParamAccess.item), "Style", "S",
            "The surface style to apply. Uses Civil 3D default if not provided.",
            GH_ParamAccess.item);
        pManager[4].Optional = true;
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilTinVolumeSurface(), "Volume Surface", "VolSrf",
            "The created Volume Surface.", GH_ParamAccess.item);

        pManager.AddTextParameter("Name", "N",
            "The name of the created surface (useful when auto-generated).", GH_ParamAccess.item);

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "Id", "Id",
            "The ObjectId of the created surface.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Cut Volume", "Cut",
            "Calculated cut volume.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Fill Volume", "Fill",
            "Calculated fill volume.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Net Volume", "Net",
            "Calculated net volume.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        if (ShouldSkipSolve()) return;

        // 1. Read all inputs first
        AutocadDocument? autocadDocument = null;
        GH_CivilTinSurface? baseSurfaceGoo = null;
        GH_CivilTinSurface? comparisonSurfaceGoo = null;
        var surfaceName = string.Empty;
        GH_CivilSurfaceStyle? styleGoo = null;

        DA.GetData(0, ref autocadDocument);

        var document = this.GetDocumentOrDefault(autocadDocument);

        if (document is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No active AutoCAD document available");
            return;
        }

        if (!DA.GetData(1, ref baseSurfaceGoo) || baseSurfaceGoo is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No base surface provided");
            return;
        }

        if (!DA.GetData(2, ref comparisonSurfaceGoo) || comparisonSurfaceGoo is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No comparison surface provided");
            return;
        }

        DA.GetData(3, ref surfaceName);
        DA.GetData(4, ref styleGoo);

        var baseSurfaceId = baseSurfaceGoo.Reference.ObjectId;
        var comparisonSurfaceId = comparisonSurfaceGoo.Reference.ObjectId;

        // 2. Build input signature for change detection
        var signature = new InputSignatureBuilder()
            .Add(baseSurfaceId)
            .Add(comparisonSurfaceId)
            .Add(surfaceName)
            .Add(styleGoo?.Value?.Id)
            .Build();

        // 3. Check for reuse to prevent infinite loops
        if (TryReuseLastCreated(signature))
        {
            var trackedId = GetFirstTrackedObjectId();
            if (trackedId != null)
            {
                var result = RetrieveVolumeSurfaceWithProperties(trackedId, document);
                if (result.volumeSurface != null)
                {
                    DA.SetData(0, new GH_CivilTinVolumeSurface(result.volumeSurface));
                    DA.SetData(1, result.volumeSurface.Name);
                    DA.SetData(2, new GH_AutocadObjectId(trackedId));
                    DA.SetData(3, result.cut);
                    DA.SetData(4, result.fill);
                    DA.SetData(5, result.net);
                    return;
                }
            }
            // Fall through to create if retrieval failed
        }

        // 4. Delete previous objects now (if replace enabled)
        DeleteTrackedObjectsIfReplaceEnabled();

        _errorMessage = string.Empty;

        var transactionManager = document.CreateTransactionManager();

        var createResult = transactionManager.PerformTask(() =>
        {
            try
            {
                var finalName = string.IsNullOrWhiteSpace(surfaceName)
                    ? AutoNamer.GenerateUniqueVolumeSurfaceName(transactionManager.AutocadDatabase, _ghPrefix)
                    : surfaceName;

                // Get style ObjectId if provided
                IObjectId? styleId = null;
                if (styleGoo?.Value != null)
                {
                    styleId = styleGoo.Value.Id;
                }

                // Create the TIN Volume Surface
                // Note: CutFactor and FillFactor are read-only in Civil 3D API and default to 1.0
                var volumeSurface = TinVolumeSurfaceCreator.Create(
                    transactionManager,
                    baseSurfaceId,
                    comparisonSurfaceId,
                    finalName,
                    styleId);

                if (volumeSurface == null)
                {
                    _errorMessage = "Failed to create TIN Volume Surface. Check that the base and comparison surfaces are valid.";
                    return (null, string.Empty, ObjectId.Null, 0.0, 0.0, 0.0);
                }

                var volumeProps = volumeSurface.GetVolumeProperties();
                var adjustedCut = volumeProps.AdjustedCutVolume;
                var adjustedFill = volumeProps.AdjustedFillVolume;
                var adjustedNet = adjustedCut - adjustedFill;

                return (volumeSurface, finalName, volumeSurface.Id, adjustedCut, adjustedFill, adjustedNet);
            }
            catch (Autodesk.Civil.CivilException ex)
            {
                _errorMessage = $"Civil 3D error: {ex.Message}";
                return (null, string.Empty, ObjectId.Null, 0.0, 0.0, 0.0);
            }
            catch (System.Exception ex)
            {
                _errorMessage = $"Failed to create TIN Volume Surface: {ex.Message}";
                return (null, string.Empty, ObjectId.Null, 0.0, 0.0, 0.0);
            }
        });

        if (!string.IsNullOrEmpty(_errorMessage))
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, _errorMessage);
            return;
        }

        var (volumeSrf, createdName, objectId, cut, fill, net) = createResult;

        if (volumeSrf == null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to create TIN Volume Surface");
            return;
        }

        // Track the created object for potential replacement
        this.TrackCreatedObject(objectId, document);

        // Set outputs
        DA.SetData(0, new GH_CivilTinVolumeSurface(volumeSrf));
        DA.SetData(1, createdName);
        DA.SetData(2, new GH_AutocadObjectId(new AutocadObjectIdWrapper(objectId)));
        DA.SetData(3, cut);
        DA.SetData(4, fill);
        DA.SetData(5, net);
    }

    /// <summary>
    /// Retrieves a TIN volume surface and calculates its volume properties.
    /// Note: This method must remain here rather than in base class because it needs
    /// to calculate volume properties within the transaction scope.
    /// </summary>
    private (TinVolumeSurface? volumeSurface, double cut, double fill, double net) RetrieveVolumeSurfaceWithProperties(
        IObjectId objectId, IAutocadDocument document)
    {
        TinVolumeSurface? volumeSurface = null;
        double cut = 0, fill = 0, net = 0;

        var transactionManager = document.CreateTransactionManager();
        transactionManager.PerformTask(() =>
        {
            try
            {
                var id = objectId.Unwrap();
                if (id.IsNull || id.IsEffectivelyErased)
                    return false;

                var transaction = transactionManager.Unwrap();
                var dbObject = transaction.GetObject(id, OpenMode.ForRead, false);

                volumeSurface = dbObject as TinVolumeSurface;
                if (volumeSurface != null)
                {
                    var volumeProps = volumeSurface.GetVolumeProperties();
                    cut = volumeProps.AdjustedCutVolume;
                    fill = volumeProps.AdjustedFillVolume;
                    net = cut - fill;
                }

                return true;
            }
            catch
            {
                return false;
            }
        });

        return (volumeSurface, cut, fill, net);
    }
}
