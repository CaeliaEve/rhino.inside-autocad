using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using Color = System.Drawing.Color;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that gets or sets properties of an AutoCAD Block Reference.
/// </summary>
[ComponentVersion(introduced: "1.0.0", updated: "1.0.20")]
public class AutocadBlockReferenceComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.secondary;

    /// <inheritdoc />
    public override Guid ComponentGuid => new("ac5cdf28-ef75-4c47-9147-15c12a61ab80");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.AutocadBlockReferenceComponent;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutocadBlockReferenceComponent"/> class.
    /// </summary>
    public AutocadBlockReferenceComponent()
        : base("AutoCAD Block Reference", "AC-BlkRef",
            "Gets or sets information on an AutoCAD Block Reference",
            "AutoCAD", "Blocks")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadBlockReference(), "BlockReference",
            "BlockReference", "An AutoCAD Block Reference", GH_ParamAccess.item);

        pManager.AddPointParameter("Origin", "Origin",
            "New insertion point for the Block Reference (Rhino units)", GH_ParamAccess.item);
        pManager[1].Optional = true;

        pManager.AddNumberParameter("Rotation", "Rot",
            "New rotation angle in radians", GH_ParamAccess.item);
        pManager[2].Optional = true;

        pManager.AddParameter(new Param_AutocadScale(GH_ParamAccess.item), "Scale", "Scale",
            "New scale for the Block Reference", GH_ParamAccess.item);
        pManager[3].Optional = true;

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "LayerId", "Layer",
            "New layer object ID", GH_ParamAccess.item);
        pManager[4].Optional = true;

        pManager.AddColourParameter("Color", "Col",
            "New color for the Block Reference", GH_ParamAccess.item);
        pManager[5].Optional = true;

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "LinetypeId", "LT",
            "New linetype object ID", GH_ParamAccess.item);
        pManager[6].Optional = true;
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Name", "Name",
            "The name of the AutoCAD Block Reference.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "Id", "Id",
            "The Id of the AutoCAD Block Reference.", GH_ParamAccess.item);

        pManager.AddPointParameter("Origin", "Origin",
            "The origin point of the Block Reference. Note this has been converted to the Rhino Units",
            GH_ParamAccess.item);

        pManager.AddNumberParameter("Rotation", "Rotation", "The Rotation of the Block Reference.",
            GH_ParamAccess.item);

        pManager.AddParameter(new Param_AutocadScale(GH_ParamAccess.item), "Scale", "Scale", "The Scale of the Block Reference. This will take either one uniform number or three numbers for a non uniform scale",
            GH_ParamAccess.item);

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "BlockDefinitionId",
            "DefId", "The Object Id  of the AutoCAD Block Definition that this BlockReference is a reference of.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_DynamicBlockReferenceProperty(GH_ParamAccess.list),
            "Properties", "P", "The Dynamic Block Reference Properties", GH_ParamAccess.list);

        pManager.AddParameter(new Param_BlockAttributeReference(GH_ParamAccess.list),
            "Attributes", "Attr", "The Block Reference Attributes", GH_ParamAccess.list);

        pManager.AddColourParameter("Color", "Col",
            "The color of the Block Reference", GH_ParamAccess.item);

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "LayerId", "Layer",
            "The layer object ID", GH_ParamAccess.item);

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "LinetypeId", "LT",
            "The linetype object ID", GH_ParamAccess.item);
    }

    /// <summary>
    /// Updates the properties of an AutoCAD Block Reference. Returns a new Wrapper with updated values.
    /// If the update fails, the original block reference is returned and an error message is added
    /// to the component.
    /// </summary>
    private AutocadBlockReferenceWrapper UpdateBlockReference(
        IAutocadTransactionManager transactionManagerWrapper,
        IAutocadBlockReference blockRef,
        Rhino.Geometry.Point3d newPosition,
        double newRotation,
        IAutocadScale newScale,
        IObjectId newLayerId,
        IColor newColor,
        IObjectId newLinetypeId)
    {

        var cadBlockRefId = blockRef.Id.Unwrap();

        var transaction = transactionManagerWrapper.Unwrap();

        var cadBlockRef =
            transaction.GetObject(cadBlockRefId, OpenMode.ForWrite) as BlockReference;

        cadBlockRef!.Position = newPosition.ToAutocadPoint3d();

        cadBlockRef.Rotation = newRotation;

        cadBlockRef.ScaleFactors = new Scale3d(newScale.X, newScale.Y, newScale.Z);

        cadBlockRef.LayerId = newLayerId.Unwrap();

        cadBlockRef.Color = Autodesk.AutoCAD.Colors.Color.FromRgb(
            newColor.Red, newColor.Green, newColor.Blue);

        cadBlockRef.LinetypeId = newLinetypeId.Unwrap();

        return new AutocadBlockReferenceWrapper(cadBlockRef);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        AutocadBlockReferenceWrapper? blockReferenceWrapper = null;

        if (!DA.GetData(0, ref blockReferenceWrapper)
            || blockReferenceWrapper is null) return;

        var newPosition = blockReferenceWrapper.Position;
        var newRotation = blockReferenceWrapper.Rotation;
        var newScale = blockReferenceWrapper.Scale as AutocadScale ?? new AutocadScale(1);
        var newLayerId = blockReferenceWrapper.LayerId;
        var newColor = blockReferenceWrapper.Color;
        var newLinetypeId = blockReferenceWrapper.LinetypeId;

        DA.GetData(1, ref newPosition);
        DA.GetData(2, ref newRotation);
        DA.GetData(3, ref newScale);
        DA.GetData(4, ref newLayerId);
        DA.GetData(5, ref newColor);
        DA.GetData(6, ref newLinetypeId);

        var change = !newPosition.Equals(blockReferenceWrapper.Position)
                     || Math.Abs(newRotation - blockReferenceWrapper.Rotation) > 1e-10
                     || !newScale.IsEqualTo(blockReferenceWrapper.Scale)
                     || !newLayerId.Equals(blockReferenceWrapper.LayerId)
                     || !newColor.IsEqualTo(blockReferenceWrapper.Color)
                     || !newLinetypeId.Equals(blockReferenceWrapper.LinetypeId);

        var document = RhinoInsideAutoCadExtension.Application.RhinoInsideManager
            .AutoCadInstance.ActiveDocument;

        var transactionManagerWrapper = document.CreateTransactionManager();

        var result = transactionManagerWrapper.PerformTask(() =>
        {

            if (change)
            {
                blockReferenceWrapper = this.UpdateBlockReference(
                    transactionManagerWrapper,
                    blockReferenceWrapper,
                    newPosition,
                    newRotation,
                    newScale,
                    newLayerId,
                    newColor,
                    newLinetypeId);
            }

            var dynamicProperties =
                blockReferenceWrapper.GetDynamicProperties(transactionManagerWrapper);

            var gooProperties = dynamicProperties.Select(property =>
                new GH_DynamicBlockReferenceProperty(property));

            var attributesSet =
                blockReferenceWrapper.GetAttributes(transactionManagerWrapper);

            var gooAttributes = attributesSet.Select(property =>
                new GH_BlockAttributeReference(property));

            return new { GooProperties = gooProperties, GooAttributes = gooAttributes };
        });

        var blockTableRecordIdGoo =
            new GH_AutocadObjectId(blockReferenceWrapper.BlockTableRecordId);

        var color = blockReferenceWrapper.Color;
        var gooColor = new GH_Colour(Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue));

        DA.SetData(0, blockReferenceWrapper.Name);
        DA.SetData(1, blockReferenceWrapper.Id);
        DA.SetData(2, blockReferenceWrapper.Position);
        DA.SetData(3, blockReferenceWrapper.Rotation);
        DA.SetData(4, blockReferenceWrapper.Scale);
        DA.SetData(5, blockTableRecordIdGoo);
        DA.SetDataList(6, result.GooProperties);
        DA.SetDataList(7, result.GooAttributes);
        DA.SetData(8, gooColor);
        DA.SetData(9, new GH_AutocadObjectId(blockReferenceWrapper.LayerId));
        DA.SetData(10, new GH_AutocadObjectId(blockReferenceWrapper.LinetypeId));
    }
}
