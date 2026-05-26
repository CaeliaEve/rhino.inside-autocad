using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;
using CivilVolumeSurface = Autodesk.Civil.DatabaseServices.TinVolumeSurface;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D TIN Volume Surfaces.
/// </summary>
public class Param_CivilTinVolumeSurface : Param_AutocadObjectBase<GH_CivilTinVolumeSurface, CivilVolumeSurface>
{
    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("A7E4B8D2-5C3F-4A9E-B1D6-8F2C9A7E3B5D");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_CivilTinVolumeSurface;

    /// <inheritdoc />
    protected override string SingularPromptMessage => "Select a Civil3d Volume Surface";

    /// <inheritdoc />
    protected override string PluralPromptMessage => "Select Civil3d Volume Surfaces";

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilTinVolumeSurface"/> class.
    /// </summary>
    public Param_CivilTinVolumeSurface()
        : base("Civil3d TIN Volume Surface", "CVL-VolSrf",
            "A TIN Volume Surface in Civil3d", "Params", "Civil3d")
    { }

    /// <inheritdoc />
    protected override IObjectFilter CreateSelectionFilter() => new CivilTinVolumeSurfaceFilter();

    /// <inheritdoc />
    protected override GH_CivilTinVolumeSurface WrapEntity(CivilVolumeSurface entity)
    {
        return new GH_CivilTinVolumeSurface(entity);
    }
}
