using Autodesk.AutoCAD.DatabaseServices;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Civil.Interop.Constants;
using Rhino.Inside.AutoCAD.Civil.Interop.Naming;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;
using CivilAssembly = Autodesk.Civil.DatabaseServices.Assembly;
using RhinoCurve = Rhino.Geometry.Curve;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that creates a Civil 3D Subassembly from a Rhino curve.
/// </summary>
/// <remarks>
/// This component converts a Rhino curve representing a cross-section profile into
/// a Civil 3D Subassembly that can be attached to an Assembly.
/// The curve's X coordinate represents the offset from the baseline, and the Z coordinate
/// represents the elevation. The Y coordinate is ignored (along alignment direction).
/// </remarks>
[ComponentVersion(introduced: "1.2.20")]
public class CreateCivilSubassemblyComponent : RhinoInsideAutocad_CreateComponentBase
{
    private string _errorMessage = string.Empty;
    private const string GhPrefix = CivilConstants.GhSubassemblyPrefix;

    /// <inheritdoc />
    public override Guid ComponentGuid => new("D8E9F0A1-2B3C-4D5E-6F7A-8B9C0D1E2F3A");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.tertiary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CreateCivilSubassemblyComponent;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCivilSubassemblyComponent"/> class.
    /// </summary>
    public CreateCivilSubassemblyComponent()
        : base("Create Civil3d Subassembly", "CVL-CreateSub",
            "Creates a Civil 3D Subassembly from a Rhino curve representing a cross-section profile",
            "Civil3d", "Assemblies")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadDocument(GH_ParamAccess.item), "Document",
            "Doc", "An AutoCAD Document. If not provided, the active document will be used.", GH_ParamAccess.item);
        pManager[0].Optional = true;

        pManager.AddParameter(new Param_CivilAssembly(), "Assembly", "Asm",
            "The Civil 3D Assembly to attach the subassembly to.", GH_ParamAccess.item);

        pManager.AddCurveParameter("Curve", "C",
            "The Rhino curve representing the cross-section profile. X = offset from baseline, Z = elevation.",
            GH_ParamAccess.item);

        pManager.AddTextParameter("Name", "N",
            "The name for the Subassembly. If not provided, a unique name will be auto-generated.",
            GH_ParamAccess.item);
        pManager[3].Optional = true;

        pManager.AddIntegerParameter("Side", "S",
            "The side of the assembly baseline: 0=None, 1=Left, 2=Right (default: 0).",
            GH_ParamAccess.item, 0);
        pManager[4].Optional = true;

        pManager.AddTextParameter("Point Code", "PC",
            "The code to assign to all points in the subassembly (default: 'P').",
            GH_ParamAccess.item, "P");
        pManager[5].Optional = true;

        pManager.AddTextParameter("Link Code", "LC",
            "The code to assign to all links in the subassembly (default: 'L').",
            GH_ParamAccess.item, "L");
        pManager[6].Optional = true;

        pManager.AddBooleanParameter("Closed", "Cl",
            "Whether to close the shape back to the first point (default: false).",
            GH_ParamAccess.item, false);
        pManager[7].Optional = true;
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilSubassemblyProperties(GH_ParamAccess.item), "Subassembly", "Sub",
            "The created Subassembly.", GH_ParamAccess.item);

        pManager.AddTextParameter("Name", "N",
            "The name of the created subassembly (useful when auto-generated).", GH_ParamAccess.item);

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "Id", "Id",
            "The ObjectId of the created subassembly.", GH_ParamAccess.item);

        pManager.AddCurveParameter("Geometry", "G",
            "The geometry of the created subassembly as curves.", GH_ParamAccess.list);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        if (this.ShouldSkipSolve()) return;

        AutocadDocument? autocadDocument = null;
        GH_CivilAssembly? assemblyGoo = null;
        RhinoCurve? curve = null;
        var subassemblyName = string.Empty;
        var side = 0;
        var pointCode = "P";
        var linkCode = "L";
        var closed = false;

        DA.GetData(0, ref autocadDocument);

        var document = this.GetDocumentOrDefault(autocadDocument);

        if (document is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No active AutoCAD document available");
            return;
        }

        if (!DA.GetData(1, ref assemblyGoo) || assemblyGoo?.Value is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No assembly provided");
            return;
        }

        if (!DA.GetData(2, ref curve) || curve is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No curve provided");
            return;
        }

        DA.GetData(3, ref subassemblyName);
        DA.GetData(4, ref side);
        DA.GetData(5, ref pointCode);
        DA.GetData(6, ref linkCode);
        DA.GetData(7, ref closed);

        // Validate side value
        if (side < 0 || side > 2)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                "Invalid side value. Must be 0 (None), 1 (Left), or 2 (Right).");
            return;
        }

        _errorMessage = string.Empty;

        var transactionManager = document.CreateTransactionManager();

        var result = transactionManager.PerformTask(() =>
        {
            try
            {
                var transaction = transactionManager.Unwrap();

                // Get the assembly
                var assemblyId = assemblyGoo.Reference.ObjectId.Unwrap();
                var assembly = transaction.GetObject(assemblyId, OpenMode.ForRead) as CivilAssembly;

                if (assembly == null)
                {
                    _errorMessage = "Failed to access the specified assembly.";
                    return (null, string.Empty, ObjectId.Null);
                }

                // Generate unique name if not provided
                var finalName = string.IsNullOrWhiteSpace(subassemblyName)
                    ? AutoNamer.GenerateUniqueSubassemblyName(transactionManager, assemblyId, GhPrefix)
                    : subassemblyName;

                // Convert side to CivilSide enum
                var civilSide = (Core.CivilSide)side;

                // Create the subassembly
                var subassembly = SubassemblyCreator.Create(
                    transactionManager,
                    curve,
                    assemblyId,
                    finalName,
                    civilSide,
                    pointCode,
                    linkCode,
                    closed);

                if (subassembly == null)
                {
                    _errorMessage = "Failed to create subassembly.";
                    return (null, string.Empty, ObjectId.Null);
                }

                return (subassembly, finalName, subassembly.SubassemblyId.Unwrap());
            }
            catch (NotSupportedException ex)
            {
                _errorMessage = ex.Message;
                return (null, string.Empty, ObjectId.Null);
            }
            catch (Autodesk.Civil.CivilException ex)
            {
                _errorMessage = $"Civil 3D error: {ex.Message}";
                return (null, string.Empty, ObjectId.Null);
            }
            catch (System.Exception ex)
            {
                _errorMessage = $"Failed to create subassembly: {ex.Message}";
                return (null, string.Empty, ObjectId.Null);
            }
        });

        if (!string.IsNullOrEmpty(_errorMessage))
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, _errorMessage);
            return;
        }

        var (createdSubassembly, createdName, objectId) = result;

        if (createdSubassembly == null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to create subassembly");
            return;
        }

        // Track the created object for potential replacement
        this.TrackCreatedObject(objectId, document);

        // Set outputs
        DA.SetData(0, new GH_CivilSubassembly(createdSubassembly));
        DA.SetData(1, createdName);
        DA.SetData(2, new GH_AutocadObjectId(new AutocadObjectIdWrapper(objectId)));
        DA.SetDataList(3, createdSubassembly.Geometry);
    }
}
