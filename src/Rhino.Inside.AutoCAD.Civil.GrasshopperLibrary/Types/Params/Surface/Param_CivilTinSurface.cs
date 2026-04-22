using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using CivilSurface = Autodesk.Civil.DatabaseServices.TinSurface;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for AutoCAD 3D solids.
/// </summary>
public class Param_CivilTinSurface : Param_AutocadObjectBase<GH_CivilTinSurface, CivilSurface>
{
    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("F82C6EC3-92F4-4E80-B5DF-15A51114551F");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Civil.GrasshopperLibrary.Properties.Resources.Param_CivilTinSurface;

    /// <inheritdoc />
    protected override string SingularPromptMessage => "Select a Civil3d Surface";

    /// <inheritdoc />
    protected override string PluralPromptMessage => "Select Civil3d Surfaces";

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilTinSurface"/> class.
    /// </summary>
    public Param_CivilTinSurface()
        : base("Civil3d TIN Surface", "CVL-Srf",
            "A TIN Surface in Civil3d", "Params", "Civil3d")
    { }

    /// <inheritdoc />
    protected override IObjectFilter CreateSelectionFilter() => new CivilTinSurfaceFilter();

    /// <inheritdoc />
    protected override GH_CivilTinSurface WrapEntity(CivilSurface entity)
    {
        return new GH_CivilTinSurface(entity);
    }
}