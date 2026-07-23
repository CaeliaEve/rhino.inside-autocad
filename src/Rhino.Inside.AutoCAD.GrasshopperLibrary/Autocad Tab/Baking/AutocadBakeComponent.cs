using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using System.Collections;
using System.Windows.Forms;
using Exception = System.Exception;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that bakes AutoCAD objects to the model space.
/// The bake is triggered by the "Bake" boolean input, or by the Bake button shown
/// on the component when the "Driven Button" context menu toggle is enabled (the default).
/// </summary>
[ComponentVersion(introduced: "1.0.0", updated: "1.2.29")]
public class AutocadBakeComponent : RhinoInsideAutocad_ComponentBase, IBakingComponent
{
    private const string DrivenButtonEnabledKey = "DrivenButtonEnabled";
    private const int BakeParamIndex = 3;

    private bool _drivenButtonEnabled = true;

    // Data dam: the Bake button only runs the bake on the solve it triggered
    // (which sets _manualRunRequested). Any other expiration (input change) is
    // driven by the Bake input alone.
    private bool _manualRunRequested;

    /// <summary>
    /// Gets a value indicating whether the Bake button is shown on the component.
    /// The Bake boolean input always drives the component; the button additionally
    /// allows triggering a bake manually.
    /// </summary>
    public bool DrivenButtonEnabled => _drivenButtonEnabled;

    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("C5D7E9F1-A3B5-4C7D-9E1F-3A5B7C9D1E3F");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.AutocadBakeComponent;

    /// <inheritdoc />
    public int OutputParamTargetIndex => 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutocadBakeComponent"/> class.
    /// </summary>
    public AutocadBakeComponent()
        : base("Bake to AutoCAD", "AC-Bake",
            "Bakes objects to AutoCAD's model space",
            "AutoCAD", "Baking")
    {
    }

    /// <inheritdoc />
    public override void CreateAttributes()
    {
        m_attributes = new AutocadBakeComponentAttributes(this);
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadDocument(GH_ParamAccess.item), "Document",
            "Doc", "The AutoCAD document to bake to. If not provided, the active document will be used.", GH_ParamAccess.item);
        pManager[0].Optional = true;

        pManager.AddGenericParameter("Objects", "O",
            "The objects to bake to AutoCAD (curves, points, meshes, solids)",
            GH_ParamAccess.list);

        pManager.AddParameter(new Param_BakeSettings(GH_ParamAccess.item), "Settings",
            "S", "Optional bake settings (layer, linetype, color)", GH_ParamAccess.item);
        pManager[2].Optional = true;

        pManager.AddBooleanParameter("Bake", "Bake",
            "A boolean when true the Objects will be baked to AutoCAD", GH_ParamAccess.item);
        pManager[BakeParamIndex].Optional = true;

    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.list), "ObjectIds",
            "Ids", "The ObjectIds of the baked objects", GH_ParamAccess.list);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        var run = false;
        DA.GetData(BakeParamIndex, ref run);

        // The Bake button triggers a bake regardless of the Bake input state
        run |= _manualRunRequested;
        _manualRunRequested = false;

        if (run == false)
        {
            return;
        }

        AutocadDocument? autocadDocument = null;
        DA.GetData(0, ref autocadDocument);

        var document = this.GetDocumentOrDefault(autocadDocument);

        if (document is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No active AutoCAD document available");
            return;
        }

        var objects = new List<object>();
        if (!DA.GetDataList(1, objects) || objects.Count == 0)
            return;

        GH_BakeSettings? settingsGoo = null;
        DA.GetData(2, ref settingsGoo);

        var settings = settingsGoo?.Value;

        var converterFactory = new RhinoConvertibleFactory();

        var bakeables = new List<IAutocadBakeable>();
        foreach (var obj in objects)
        {
            var bakeable = BakeableExtractor.ExtractBakeable(obj, converterFactory);
            if (bakeable != null)
            {
                bakeables.Add(bakeable);
            }
            else
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    $"Object of type {obj?.GetType().Name ?? "null"} is not bakeable");
            }
        }

        if (bakeables.Count == 0)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No bakeable objects provided");
            return;
        }

        var bakedIds = new List<GH_AutocadObjectId>();

        var transactionManagerWrapper = document.CreateTransactionManager();

        _ = transactionManagerWrapper.PerformTask(() =>
         {
             foreach (var bakeable in bakeables)
             {
                 try
                 {
                     var objectIds = bakeable.BakeToAutocad(transactionManagerWrapper, this, settings);

                     foreach (var objectId in objectIds)
                     {
                         bakedIds.Add(new GH_AutocadObjectId(objectId));
                     }
                 }
                 catch (Exception ex)
                 {
                     this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                         $"Failed to bake object: {ex.Message}");
                 }
             }

             return true;
         });

        DA.SetDataList(0, bakedIds);
    }

    /// <summary>
    /// Appends additional menu items to the component's context menu.
    /// </summary>
    protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
    {
        base.AppendAdditionalComponentMenuItems(menu);
        Menu_AppendSeparator(menu);

        var drivenButtonItem = Menu_AppendItem(
            menu,
            "Driven Button",
            this.OnDrivenButtonMenuClick,
            true,
            _drivenButtonEnabled
        );
        drivenButtonItem.ToolTipText = "When enabled, a Bake button is shown on the component for manually triggering a bake. The Bake input always drives the component.";
    }

    /// <summary>
    /// Handles the click event for the Driven Button menu item.
    /// Only the button visibility changes, so the layout is refreshed without
    /// recomputing the solution (which could re-bake with the Bake input held true).
    /// </summary>
    private void OnDrivenButtonMenuClick(object? sender, EventArgs e)
    {
        _drivenButtonEnabled = !_drivenButtonEnabled;
        this.Attributes?.ExpireLayout();
        Grasshopper.Instances.ActiveCanvas?.Invalidate();
    }

    /// <summary>
    /// Triggers a manual bake of the component.
    /// Called by the custom attributes when the Bake button is clicked.
    /// </summary>
    public void TriggerManualRun()
    {
        _manualRunRequested = true;
        this.ExpireSolution(true);
    }

    /// <inheritdoc />
    public override bool Read(GH_IReader reader)
    {
        if (!base.Read(reader))
            return false;

        _drivenButtonEnabled = true;
        reader.TryGetBoolean(DrivenButtonEnabledKey, ref _drivenButtonEnabled);

        return true;
    }

    /// <inheritdoc />
    public override bool Write(GH_IWriter writer)
    {
        if (!base.Write(writer))
            return false;

        writer.SetBoolean(DrivenButtonEnabledKey, _drivenButtonEnabled);

        return true;
    }

    /// <inheritdoc />
    public void AddWarningMessage(string message)
    {
        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, message);
    }

    /// <inheritdoc />
    public bool AppendDataList(IEnumerable list)
    {
        var ghParam = this.Params.Output[this.OutputParamTargetIndex];

        var path = new GH_Path(0);

        if (ghParam.VolatileData.PathCount > 0)
        {
            var lastPath = ghParam.VolatileData.Paths[ghParam.VolatileData.PathCount - 1];

            var indices = lastPath.Indices;

            indices[indices.Length - 1]++;

            path = new GH_Path(indices);
        }

        var result = ghParam.AddVolatileDataList(path, list);

        if (result)
        {
            ghParam.ExpireSolution(false);

            this.OnPingDocument()?.NewSolution(false);
        }

        return result;
    }
}
