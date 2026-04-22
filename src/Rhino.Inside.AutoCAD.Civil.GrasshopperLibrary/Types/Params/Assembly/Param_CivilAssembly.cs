using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using CivilAssembly = Autodesk.Civil.DatabaseServices.Assembly;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D Assemblies.
/// </summary>
public class Param_CivilAssembly : Param_AutocadObjectBase<GH_CivilAssembly, CivilAssembly>
{
    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("31d6c09b-3dc7-46f8-9521-521be02de751");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Civil.GrasshopperLibrary.Properties.Resources.Param_CivilAssembly;

    /// <inheritdoc />
    protected override string SingularPromptMessage => "Select a Civil3d Assembly";

    /// <inheritdoc />
    protected override string PluralPromptMessage => "Select Civil3d Assemblies";

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAssembly"/> class.
    /// </summary>
    public Param_CivilAssembly()
        : base("Civil3d Assembly", "CVL-Asm",
            "A Civil 3D Assembly", "Params", "Civil3d")
    { }

    /// <inheritdoc />
    protected override IObjectFilter CreateSelectionFilter() => new CivilAssemblyFilter();

    /// <inheritdoc />
    protected override GH_CivilAssembly WrapEntity(CivilAssembly entity)
    {
        return new GH_CivilAssembly(entity);
    }
}
