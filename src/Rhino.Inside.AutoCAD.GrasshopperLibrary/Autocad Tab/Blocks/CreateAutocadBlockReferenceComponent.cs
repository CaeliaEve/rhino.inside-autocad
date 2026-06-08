using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that adds AutoCAD Block References to a document.
/// </summary>
[ComponentVersion(introduced: "1.0.0", updated: "1.0.21")]
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
            "Doc", "An AutoCAD Document. If not provided, the active document will be used.", GH_ParamAccess.item);
        pManager[0].Optional = true;

        pManager.AddParameter(new Param_AutocadBlockTableRecord(GH_ParamAccess.item), "BlockDefinition",
            "BlockDef", "The Block Definition to insert", GH_ParamAccess.item);

        pManager.AddPointParameter("InsertionPoints", "Points",
            "The insertion point(s) for the Block Reference(s), as Rhino Points", GH_ParamAccess.list);

        pManager.AddNumberParameter("Rotation", "Rot",
            "The rotation angle in radians", GH_ParamAccess.item, 0.0);
        pManager[3].Optional = true;

        pManager.AddParameter(new Param_AutocadScale(GH_ParamAccess.item), "Scale", "Scale", "The Scale of the Block Reference. This will take either one uniform number or three numbers for a non uniform scale",
            GH_ParamAccess.item);
        pManager[4].Optional = true;

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "LayerId", "Layer",
            "The layer object ID for the Block Reference", GH_ParamAccess.item);
        pManager[5].Optional = true;

        pManager.AddParameter(new Param_AutocadColor(GH_ParamAccess.item), "Color", "Col",
            "The color for the Block Reference", GH_ParamAccess.item);
        pManager[6].Optional = true;

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "LinetypeId", "LT",
            "The linetype object ID for the Block Reference", GH_ParamAccess.item);
        pManager[7].Optional = true;
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadBlockReference(),
            "BlockReferences", "Refs", "The created AutoCAD Block Reference(s)",
            GH_ParamAccess.list);
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

        var rotation = 0.0;
        var scale = new AutocadScale(1);
        DA.GetData(3, ref rotation);
        DA.GetData(4, ref scale);

        IObjectId? layerId = null;
        AutocadColorWrapper? color = null;
        IObjectId? linetypeId = null;

        DA.GetData(5, ref layerId);
        DA.GetData(6, ref color);
        DA.GetData(7, ref linetypeId);

        // 2. Build input signature for change detection
        var signature = new InputSignatureBuilder()
            .Add(blockTableRecord.Id)
            .AddPoints(insertionPoints)
            .Add(rotation)
            .AddScale(scale)
            .Add(layerId)
            .AddColor(color)
            .Add(linetypeId)
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

            foreach (var rhinoPoint in insertionPoints)
            {

                var insertionPoint = rhinoPoint.ToAutocadPoint3d();

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
