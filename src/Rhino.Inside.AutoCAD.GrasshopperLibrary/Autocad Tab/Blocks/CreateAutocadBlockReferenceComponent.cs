using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that adds AutoCAD Block References to a document.
/// </summary>
[ComponentVersion(introduced: "1.0.0", updated: "1.2.29")]
public class CreateAutocadBlockReferenceComponent : RhinoInsideAutocad_CreateComponentBase
{

    /// <inheritdoc />
    public override Guid ComponentGuid => new("c7f3a2e8-9d4b-5c6f-8e1a-2b3c4d5e6f7a");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.secondary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.AddAutocadBlockReferenceComponent;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateAutocadBlockReferenceComponent"/> class.
    /// </summary>
    public CreateAutocadBlockReferenceComponent()
        : base("Create AutoCAD Block Reference", "AC-BlkRef",
            "Creates AutoCAD Block Reference(s) to a document at the specified insertion point(s). The Block is created on memory to add to the Autocad Document use the Autocad Bake Component",
            "AutoCAD", "Blocks")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadDocument(GH_ParamAccess.item), "Document",
            "Doc", "An AutoCAD Document. If not provided, the active document will be used.",
            GH_ParamAccess.item);
        pManager[0].Optional = true;

        pManager.AddParameter(new Param_AutocadBlockTableRecord(GH_ParamAccess.item), "BlockDefinition",
            "BlockDef", "The Block Definition to insert", GH_ParamAccess.item);

        pManager.AddPointParameter("InsertionPoints", "Points",
            "The insertion point(s) for the Block Reference(s), as Rhino Points", GH_ParamAccess.list);

        pManager.AddNumberParameter("Rotation", "Rot",
            "The rotation angle(s) in radians. Matched one-for-one with the insertion points; the last value is repeated if the list is shorter",
            GH_ParamAccess.list);
        pManager[3].Optional = true;

        pManager.AddParameter(new Param_AutocadScale(GH_ParamAccess.list), "Scale", "Scale",
            "The Scale(s) of the Block Reference(s). Each Scale will take either one uniform number or three numbers for a non uniform scale. Matched one-for-one with the insertion points; the last value is repeated if the list is shorter",
            GH_ParamAccess.list);
        pManager[4].Optional = true;

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.list), "LayerId", "Layer",
            "The layer object ID(s) for the Block Reference(s). Matched one-for-one with the insertion points; the last value is repeated if the list is shorter",
            GH_ParamAccess.list);
        pManager[5].Optional = true;

        pManager.AddParameter(new Param_AutocadColor(GH_ParamAccess.list), "Color", "Col",
            "The color(s) for the Block Reference(s). Matched one-for-one with the insertion points; the last value is repeated if the list is shorter",
            GH_ParamAccess.list);
        pManager[6].Optional = true;

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.list), "LinetypeId", "LT",
            "The linetype object ID(s) for the Block Reference(s). Matched one-for-one with the insertion points; the last value is repeated if the list is shorter",
            GH_ParamAccess.list);
        pManager[7].Optional = true;
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadBlockReference(),
            "BlockReferences", "Refs", "The created AutoCAD Block Reference(s)",
            GH_ParamAccess.list);
    }

    /// <summary>
    /// Returns the value at <paramref name="index"/>, repeating the last value if the list is
    /// shorter, or <paramref name="fallback"/> if the list is empty.
    /// </summary>
    private T GetValueAtOrLast<T>(IReadOnlyList<T> list, int index, T fallback)
        => list.Count == 0 ? fallback : list[Math.Min(index, list.Count - 1)];

    /// <summary>
    /// Adds a runtime warning when an optional list input's length is neither 1 nor the
    /// insertion point count, since values are matched one-for-one with the insertion points.
    /// </summary>
    private void WarnIfLengthMismatch(int listCount, int pointCount, string inputName)
    {
        if (listCount > 1 && listCount != pointCount)
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                $"{inputName} list length ({listCount}) does not match the InsertionPoints length ({pointCount}). " +
                "Values are matched one-for-one; the last value is repeated if the list is shorter, extra values are ignored.");
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        // Skip solve if undo/redo deferral is active (see base class documentation)
        if (this.ShouldSkipSolve())
            return;

        // 1. Read all inputs first
        AutocadDocument? autocadDocument = null;
        DA.GetData(0, ref autocadDocument);

        var document = this.GetDocumentOrDefault(autocadDocument);

        if (document is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No active AutoCAD document available");
            return;
        }

        AutocadBlockTableRecordWrapper? blockTableRecord = null;
        if (!DA.GetData(1, ref blockTableRecord) || blockTableRecord is null) return;

        var insertionPoints = new List<Rhino.Geometry.Point3d>();
        if (!DA.GetDataList(2, insertionPoints) || insertionPoints.Count == 0) return;

        var rotations = new List<double>();
        var scales = new List<AutocadScale>();
        var layerIds = new List<IObjectId?>();
        var colors = new List<AutocadColorWrapper?>();
        var linetypeIds = new List<IObjectId?>();

        DA.GetDataList(3, rotations);
        DA.GetDataList(4, scales);
        DA.GetDataList(5, layerIds);
        DA.GetDataList(6, colors);
        DA.GetDataList(7, linetypeIds);

        this.WarnIfLengthMismatch(rotations.Count, insertionPoints.Count, "Rotation");
        this.WarnIfLengthMismatch(scales.Count, insertionPoints.Count, "Scale");
        this.WarnIfLengthMismatch(layerIds.Count, insertionPoints.Count, "LayerId");
        this.WarnIfLengthMismatch(colors.Count, insertionPoints.Count, "Color");
        this.WarnIfLengthMismatch(linetypeIds.Count, insertionPoints.Count, "LinetypeId");

        // 2. Build input signature for change detection
        var signature = new InputSignatureBuilder()
            .Add(blockTableRecord.Id)
            .AddPoints(insertionPoints)
            .AddDoubles(rotations)
            .AddScales(scales)
            .AddObjectIds(layerIds)
            .AddColors(colors)
            .AddObjectIds(linetypeIds)
            .Build();

        // 3. Check for reuse to prevent infinite loops
        if (this.TryReuseLastCreated(signature))
        {
            var retrievedBlocks = this.RetrieveAllTrackedObjects<BlockReference>(document);
            if (retrievedBlocks.Count > 0)
            {
                var wrappers = retrievedBlocks
                    .Select(br => new GH_AutocadBlockReference(new AutocadBlockReferenceWrapper(br)))
                    .ToList();
                DA.SetDataList(0, wrappers);
                return;
            }
            // Fall through to create if retrieval failed
        }

        // 4. Delete previous objects now (if replace enabled)
        this.DeleteTrackedObjectsIfReplaceEnabled();

        var blockReferences = new List<GH_AutocadBlockReference>();

        var transactionManagerWrapper = document.CreateTransactionManager();

        _ = transactionManagerWrapper.PerformTask(() =>
        {

            var modelSpace = transactionManagerWrapper.GetModelSpace(openForWrite: true);

            var modelSpaceRecord = modelSpace.Unwrap();

            var transaction = transactionManagerWrapper.Unwrap();

            var defaultScale = new AutocadScale(1);

            for (var index = 0; index < insertionPoints.Count; index++)
            {

                var insertionPoint = insertionPoints[index].ToAutocadPoint3d();

                var rotation = this.GetValueAtOrLast(rotations, index, 0.0);
                var scale = this.GetValueAtOrLast(scales, index, defaultScale);
                var layerId = this.GetValueAtOrLast(layerIds, index, null);
                var color = this.GetValueAtOrLast(colors, index, null);
                var linetypeId = this.GetValueAtOrLast(linetypeIds, index, null);

                var blockReference = new BlockReference(
                    insertionPoint,
                    blockTableRecord.Id.Unwrap());
                blockReference.Rotation = rotation;
                blockReference.ScaleFactors = new Scale3d(scale.X, scale.Y, scale.Z);

                if (layerId is not null)
                    blockReference.LayerId = layerId.Unwrap();

                blockReference.Color = color?.Unwrap() ?? AutocadColorWrapper.CreateByLayer().Unwrap();

                if (linetypeId is not null)
                    blockReference.LinetypeId = linetypeId.Unwrap();

                var objectId = modelSpaceRecord.AppendEntity(blockReference);

                transaction.AddNewlyCreatedDBObject(blockReference, true);

                // Track created object for replace-on-recompute functionality
                this.TrackCreatedObject(objectId, document);

                var cadBlockDefinition = blockTableRecord.Unwrap();

                if (cadBlockDefinition.HasAttributeDefinitions)
                {
                    foreach (var id in cadBlockDefinition)
                    {
                        var dbObject = transaction.GetObject(id, OpenMode.ForRead);
                        if (dbObject is AttributeDefinition attDef)
                        {
                            using (var attributeReference = new AttributeReference())
                            {
                                attributeReference.SetAttributeFromBlock(attDef, blockReference.BlockTransform);
                                blockReference.AttributeCollection.AppendAttribute(attributeReference);
                                transaction.AddNewlyCreatedDBObject(attributeReference, true);

                                attributeReference.RecordGraphicsModified(true);
                            }
                        }
                    }

                    blockReference.RecordGraphicsModified(true);
                }

                blockReferences.Add(
                    new GH_AutocadBlockReference(new AutocadBlockReferenceWrapper(blockReference)));
            }

            return true;
        });

        DA.SetDataList(0, blockReferences);
    }
}
