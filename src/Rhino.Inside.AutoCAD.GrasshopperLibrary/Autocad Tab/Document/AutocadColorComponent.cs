using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using Color = System.Drawing.Color;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Constructs and deconstructs AutoCAD colors with ByLayer/ByBlock support.
/// </summary>
/// <remarks>
/// This is a pass-through component that can create AutoCAD colors from various
/// inputs and deconstruct them to their constituent parts. Input priority
/// (highest to lowest): AutoCAD Color > ByLayer > ByBlock > ColorIndex > Color.
/// </remarks>
[ComponentVersion(introduced: "1.2.25")]
public class AutocadColorComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("b2a478c0-ad82-4d8b-97c0-f58eb775f066");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.AutocadColorComponent;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutocadColorComponent"/> class.
    /// </summary>
    public AutocadColorComponent()
        : base("AutoCAD Color", "AC-Color",
            "Constructs or deconstructs an AutoCAD color. " +
            "Priority: AutoCAD Color > ByLayer > ByBlock > ColorIndex > Color",
            "AutoCAD", "Document")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadColor(GH_ParamAccess.item), "AutoCAD Color", "AC",
            "An existing AutoCAD color to deconstruct. Highest priority.", GH_ParamAccess.item);
        pManager[0].Optional = true;

        pManager.AddColourParameter("Color", "C",
            "RGB color value. Lowest priority. If AutoCAD Color is set then this property will have not effect.", GH_ParamAccess.item);
        pManager[1].Optional = true;

        pManager.AddIntegerParameter("ColorIndex", "ACI",
            "AutoCAD Color Index (1-255). Special values: 256=ByLayer, 0=ByBlock. If AutoCAD Color is set then this property will have not effect.",
            GH_ParamAccess.item);
        pManager[2].Optional = true;

        pManager.AddBooleanParameter("ByBlock", "BB",
            "If true, use ByBlock color (inherits from containing block). If AutoCAD Color is set then this property will have not effect.",
            GH_ParamAccess.item);
        pManager[3].Optional = true;

        pManager.AddBooleanParameter("ByLayer", "BL",
            "If true, use ByLayer color (inherits from layer). Default if no inputs. If AutoCAD Color is set then this property will have not effect.",
            GH_ParamAccess.item);
        pManager[4].Optional = true;
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadColor(GH_ParamAccess.item), "AutoCAD Color", "AC",
            "The resulting AutoCAD color.", GH_ParamAccess.item);

        pManager.AddColourParameter("Color", "C",
            "RGB color value (resolved, may not reflect ByLayer/ByBlock).", GH_ParamAccess.item);

        pManager.AddIntegerParameter("ColorIndex", "ACI",
            "AutoCAD Color Index. Special values: 256=ByLayer, 0=ByBlock.",
            GH_ParamAccess.item);

        pManager.AddBooleanParameter("ByBlock", "BB",
            "True if color is set to ByBlock.", GH_ParamAccess.item);

        pManager.AddBooleanParameter("ByLayer", "BL",
            "True if color is set to ByLayer.", GH_ParamAccess.item);
    }

    private AutocadColorWrapper? FromIndex(int? index) => index switch
    {
        null => null,
        AutocadColorWrapper.ByLayerIndex => AutocadColorWrapper.CreateByLayer(),
        AutocadColorWrapper.ByBlockIndex => AutocadColorWrapper.CreateByBlock(),
        _ => AutocadColorWrapper.CreateFromIndex((short)index.Value),
    };

    private AutocadColorWrapper? FromRgb(Color? color) =>
        color is { } autocadColor
            ? AutocadColorWrapper.CreateFromRgb(autocadColor.R, autocadColor.G, autocadColor.B)
            : null;

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        IAutocadColor? autocadColorInput = null;
        Color? inputColor = null;
        int? colorIndex = null;
        var byBlock = false;
        var byLayer = false;

        // Get optional inputs - check AutoCAD Color first (highest priority)
        GH_AutocadColor? ghAutocadColor = null;
        if (DA.GetData(0, ref ghAutocadColor) && ghAutocadColor?.Value != null)
            autocadColorInput = ghAutocadColor.Value;

        GH_Colour? ghColor = null;
        if (DA.GetData(1, ref ghColor) && ghColor != null)
            inputColor = ghColor.Value;

        var tempIndex = 0;
        if (DA.GetData(2, ref tempIndex))
            colorIndex = tempIndex;

        DA.GetData(3, ref byBlock);
        DA.GetData(4, ref byLayer);

        // Determine color with priority: AutoCAD Color > ByLayer > ByBlock > ColorIndex > Color
        var result =
            autocadColorInput
            ?? (byLayer ? AutocadColorWrapper.CreateByLayer() : null)
            ?? (byBlock ? AutocadColorWrapper.CreateByBlock() : null)
            ?? this.FromIndex(colorIndex)
            ?? this.FromRgb(inputColor)
            ?? AutocadColorWrapper.CreateByLayer();

        var activeDoc = RhinoInsideAutoCadExtension.Application?.RhinoInsideManager
            ?.AutoCadInstance?.ActiveDocument;

        if (activeDoc is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                "No active AutoCAD document available");
            return;
        }

        var autocadDocument = activeDoc as AutocadDocument;

        if (autocadDocument is null)
            return;

        var transactionManager = autocadDocument.CreateTransactionManager();

        var trueColor = transactionManager.PerformTask(() => result.ResolveColor(transactionManager));

        var systemColor = Color.FromArgb(trueColor.Red, trueColor.Green, trueColor.Blue);

        // Output
        DA.SetData(0, new GH_AutocadColor(result));
        DA.SetData(1, new GH_Colour(systemColor));
        DA.SetData(2, (int)result.ColorIndex);
        DA.SetData(3, result.IsByBlock);
        DA.SetData(4, result.IsByLayer);
    }
}
