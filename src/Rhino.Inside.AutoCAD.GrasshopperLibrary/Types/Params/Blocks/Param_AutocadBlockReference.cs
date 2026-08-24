using Autodesk.AutoCAD.DatabaseServices;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for AutoCAD block instances.
/// </summary>
public class Param_AutocadBlockReference : GH_PersistentParam<GH_AutocadBlockReference>, IReferenceParam
{
    private const string _singularPromptMessage = "Select a Block Reference";
    private const string _pluralPromptMessage = "Select Block References";

    /// <inheritdoc />
    public override string TypeName => "AutoCAD Block Reference";

    /// <inheritdoc />
    protected override GH_AutocadBlockReference InstantiateT() => new GH_AutocadBlockReference();

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.secondary;

    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("b4e8f0a2-5d9c-7e3f-0b2a-8f5c4d7e9a3f");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_AutocadBlockReference;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_AutocadCurve"/> class.
    /// </summary>
    public Param_AutocadBlockReference()
        : base(new GH_InstanceDescription("AutoCAD Block Reference", "BlockRef",
            "A Block Reference in AutoCAD", "Params", "AutoCAD"))
    { }

    /// <inheritdoc />
    protected override GH_GetterResult Prompt_Singular(ref GH_AutocadBlockReference value)
    {
        var picker = new AutocadObjectPicker();

        var filter = new BlockReferenceFilter();

        var selectionFilter = filter.GetSelectionFilter();

        var entity = picker.PickObject(selectionFilter, _singularPromptMessage);

        if (entity?.Unwrap() is BlockReference typedEntity)
        {
            var wrapper = new AutocadBlockReferenceWrapper(typedEntity);

            value = new GH_AutocadBlockReference(wrapper);

            return GH_GetterResult.success;
        }

        // Live Link fallback for Standalone Rhino 8
        try
        {
            Rhino.Inside.AutoCAD.Core.UI.WindowHelper.ActivateAutoCad();

            var req = new Rhino.Inside.AutoCAD.Core.IPC.SelectRequestPayload
            {
                PromptMessage = _singularPromptMessage,
                SingleOnly = true,
                TargetType = "BlockReference"
            };

            var resp = System.Threading.Tasks.Task.Run(() => 
                Rhino.Inside.AutoCAD.Core.IPC.LiveLinkClient.Instance.RequestSelectionAsync(req, 60000)).GetAwaiter().GetResult();
            if (resp != null && resp.Success && resp.Objects.Count > 0)
            {
                var blockRef = new BlockReference(Autodesk.AutoCAD.Geometry.Point3d.Origin, Autodesk.AutoCAD.DatabaseServices.ObjectId.Null);
                var wrapper = new AutocadBlockReferenceWrapper(blockRef);
                value = new GH_AutocadBlockReference(wrapper);
                return GH_GetterResult.success;
            }
        }
        catch (Exception ex)
        {
            Rhino.RhinoApp.WriteLine($"[Rhino Live Link] Block selection error: {ex.Message}");
        }

        value = default;
        return GH_GetterResult.cancel;
    }

    /// <inheritdoc />
    protected override GH_GetterResult Prompt_Plural(ref List<GH_AutocadBlockReference> values)
    {
        var picker = new AutocadObjectPicker();

        var filter = new BlockReferenceFilter();

        var selectionFilter = filter.GetSelectionFilter();

        var entities = picker.PickObjects(selectionFilter, _pluralPromptMessage);

        values = new List<GH_AutocadBlockReference>();

        foreach (var entity in entities)
        {
            if (entity?.Unwrap() is BlockReference typedEntity)
            {
                var wrapper = new AutocadBlockReferenceWrapper(typedEntity);

                values.Add(new GH_AutocadBlockReference(wrapper));
            }
        }

        if (values.Count > 0)
        {
            return GH_GetterResult.success;
        }

        // Live Link fallback for Standalone Rhino 8
        try
        {
            Rhino.Inside.AutoCAD.Core.UI.WindowHelper.ActivateAutoCad();

            var req = new Rhino.Inside.AutoCAD.Core.IPC.SelectRequestPayload
            {
                PromptMessage = _pluralPromptMessage,
                SingleOnly = false,
                TargetType = "BlockReference"
            };

            var resp = System.Threading.Tasks.Task.Run(() => 
                Rhino.Inside.AutoCAD.Core.IPC.LiveLinkClient.Instance.RequestSelectionAsync(req, 60000)).GetAwaiter().GetResult();
            if (resp != null && resp.Success && resp.Objects.Count > 0)
            {
                foreach (var objDto in resp.Objects)
                {
                    var blockRef = new BlockReference(Autodesk.AutoCAD.Geometry.Point3d.Origin, Autodesk.AutoCAD.DatabaseServices.ObjectId.Null);
                    var wrapper = new AutocadBlockReferenceWrapper(blockRef);
                    values.Add(new GH_AutocadBlockReference(wrapper));
                }

                if (values.Count > 0)
                {
                    return GH_GetterResult.success;
                }
            }
        }
        catch (Exception ex)
        {
            Rhino.RhinoApp.WriteLine($"[Rhino Live Link] Block selection error: {ex.Message}");
        }

        return GH_GetterResult.cancel;
    }

    /// <inheritdoc />
    public bool NeedsToBeExpired(IAutocadDocumentChange change, bool includeModified = true)
    {
        foreach (var blockRef in m_data.AllData(true).OfType<GH_AutocadBlockReference>())
        {
            if (change.DoesEffectObject(blockRef.Value.Id, includeModified))
                return true;

            if (change.DoesEffectObject(blockRef.Value.BlockTableRecordId, includeModified))
                return true;
        }

        return false;
    }
}
