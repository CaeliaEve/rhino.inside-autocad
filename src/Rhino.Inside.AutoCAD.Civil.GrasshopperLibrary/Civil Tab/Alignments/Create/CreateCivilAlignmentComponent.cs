using Autodesk.AutoCAD.DatabaseServices;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Civil.Interop.Constants;
using Rhino.Inside.AutoCAD.Civil.Interop.Naming;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;
using CivilAlignmentType = Autodesk.Civil.DatabaseServices.AlignmentType;
using CivilDocument = Autodesk.Civil.ApplicationServices.CivilDocument;
using RhinoCurve = Rhino.Geometry.Curve;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that creates a Civil 3D Alignment from a Rhino curve.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class CreateCivilAlignmentComponent : RhinoInsideAutocad_CreateComponentBase
{
    private string _errorMessage = string.Empty;
    private const string GhPrefix = CivilConstants.GhPrefix;

    /// <inheritdoc />
    public override Guid ComponentGuid => new("C7D8E9F0-1A2B-3C4D-5E6F-7A8B9C0D1E2F");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.tertiary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CreateCivilAlignmentComponent;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCivilAlignmentComponent"/> class.
    /// </summary>
    public CreateCivilAlignmentComponent()
        : base("Create Civil3d Alignment", "CVL-CreateAln",
            "Creates a Civil 3D Alignment from a Rhino curve",
            "Civil3d", "Alignments")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadDocument(GH_ParamAccess.item), "Document",
            "Doc", "An AutoCAD Document. If not provided, the active document will be used.", GH_ParamAccess.item);
        pManager[0].Optional = true;

        pManager.AddCurveParameter("Curve", "C",
            "The Rhino curve to create the alignment from.", GH_ParamAccess.item);

        pManager.AddTextParameter("Name", "N",
            "The name for the Alignment. If not provided, a unique name will be auto-generated (e.g., GH_Alignment_001).",
            GH_ParamAccess.item);
        pManager[2].Optional = true;

        pManager.AddParameter(new Param_CivilSite(GH_ParamAccess.item), "Site", "Site",
            "The site for the alignment. If not provided, a siteless alignment will be created.", GH_ParamAccess.item);
        pManager[3].Optional = true;

        pManager.AddParameter(new Param_AutocadLayer(GH_ParamAccess.item), "Layer", "L",
            "The layer for the alignment. Uses the current layer if not provided.", GH_ParamAccess.item);
        pManager[4].Optional = true;

        pManager.AddParameter(new Param_CivilAlignmentStyle(GH_ParamAccess.item), "Style", "S",
            "The alignment style to apply. Can be a style name (string) or a style object. Uses Civil 3D default if not provided.",
            GH_ParamAccess.item);
        pManager[5].Optional = true;

        pManager.AddParameter(new Param_CivilAlignmentLabelSetStyle(GH_ParamAccess.item), "LabelSet", "LS",
            "The alignment label set style to apply. Can be a style name (string) or a style object. Uses Civil 3D default if not provided.",
            GH_ParamAccess.item);
        pManager[6].Optional = true;

        pManager.AddIntegerParameter("Type", "T",
            "Alignment type: 1=Centerline (default), 2=Offset, 3=CurbReturn, 4=Utility, 5=Rail",
            GH_ParamAccess.item, 1);
        pManager[7].Optional = true;
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilAlignment(), "Alignment", "Aln",
            "The created Alignment.", GH_ParamAccess.item);

        pManager.AddTextParameter("Name", "N",
            "The name of the created alignment (useful when auto-generated).", GH_ParamAccess.item);

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "Id", "Id",
            "The ObjectId of the created alignment.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        if (ShouldSkipSolve()) return;

        AutocadDocument? autocadDocument = null;
        RhinoCurve? curve = null;
        var alignmentName = string.Empty;
        GH_CivilSite? siteGoo = null;
        GH_AutocadLayer? layerGoo = null;
        GH_CivilAlignmentStyle? styleGoo = null;
        GH_CivilAlignmentLabelSetStyle? labelSetGoo = null;
        var alignmentType = 1;

        DA.GetData(0, ref autocadDocument);

        var document = this.GetDocumentOrDefault(autocadDocument);

        if (document is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No active AutoCAD document available");
            return;
        }

        if (!DA.GetData(1, ref curve) || curve is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No curve provided");
            return;
        }

        DA.GetData(2, ref alignmentName);
        DA.GetData(3, ref siteGoo);
        DA.GetData(4, ref layerGoo);
        DA.GetData(5, ref styleGoo);
        DA.GetData(6, ref labelSetGoo);
        DA.GetData(7, ref alignmentType);

        // Validate alignment type
        if (alignmentType < 1 || alignmentType > 5)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                "Invalid alignment type. Must be 1-5 (1=Centerline, 2=Offset, 3=CurbReturn, 4=Utility, 5=Rail)");
            return;
        }

        _errorMessage = string.Empty;

        var transactionManager = document.CreateTransactionManager();

        var result = transactionManager.PerformTask(() =>
        {
            try
            {
                var database = transactionManager.AutocadDatabase.Unwrap();
                var civilDoc = CivilDocument.GetCivilDocument(database);

                // Generate unique name if not provided
                var finalName = string.IsNullOrWhiteSpace(alignmentName)
                    ? AutoNamer.GenerateUniqueAlignmentName(transactionManager, GhPrefix)
                    : alignmentName;

                // Get site ObjectId (Null for siteless alignment)
                var siteId = ObjectId.Null;
                if (siteGoo?.Value != null)
                {
                    siteId = siteGoo.Value.Id.Unwrap();
                }

                // Get layer ObjectId (current layer if not provided)
                var layerId = database.Clayer;
                if (layerGoo?.Value != null)
                {
                    layerId = layerGoo.Value.Id.Unwrap();
                }

                // Get style ObjectId if provided
                ObjectId? styleId = null;
                if (styleGoo?.Value != null)
                {
                    styleId = styleGoo.Value.Id.Unwrap();
                }

                // Get label set ObjectId if provided
                ObjectId? labelSetId = null;
                if (labelSetGoo?.Value != null)
                {
                    labelSetId = labelSetGoo.Value.Id.Unwrap();
                }

                // Convert alignment type
                var civAlignmentType = (CivilAlignmentType)alignmentType;

                // Resolve style - use first available if not specified
                var resolvedStyleId = styleId.HasValue && !styleId.Value.IsNull
                    ? styleId.Value
                    : civilDoc.Styles.AlignmentStyles[0];

                // Resolve label set - use first available if not specified
                var resolvedLabelSetId = labelSetId.HasValue && !labelSetId.Value.IsNull
                    ? labelSetId.Value
                    : civilDoc.Styles.LabelSetStyles.AlignmentLabelSetStyles[0];

                // Create the alignment
                var alignment = AlignmentCreator.Create(
                    transactionManager,
                    curve,
                    finalName,
                    siteId,
                    layerId,
                    resolvedStyleId,
                    resolvedLabelSetId,
                    civAlignmentType);

                if (alignment == null)
                {
                    _errorMessage = "Failed to create alignment. The alignment name may already exist.";
                    return (null, string.Empty, ObjectId.Null);
                }

                return (alignment, finalName, alignment.Id);
            }
            catch (Autodesk.Civil.CivilException ex)
            {
                _errorMessage = $"Civil 3D error: {ex.Message}";
                return (null, string.Empty, ObjectId.Null);
            }
            catch (System.Exception ex)
            {
                _errorMessage = $"Failed to create alignment: {ex.Message}";
                return (null, string.Empty, ObjectId.Null);
            }
        });

        if (!string.IsNullOrEmpty(_errorMessage))
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, _errorMessage);
            return;
        }

        var (createdAlignment, createdName, objectId) = result;

        if (createdAlignment == null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to create alignment");
            return;
        }

        // Track the created object for potential replacement
        this.TrackCreatedObject(objectId, document);

        // Set outputs
        DA.SetData(0, new GH_CivilAlignment(createdAlignment));
        DA.SetData(1, createdName);
        DA.SetData(2, new GH_AutocadObjectId(new AutocadObjectIdWrapper(objectId)));
    }
}
