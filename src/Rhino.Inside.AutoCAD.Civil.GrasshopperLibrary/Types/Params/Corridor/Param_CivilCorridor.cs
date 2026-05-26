using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using CivilCorridor = Autodesk.Civil.DatabaseServices.Corridor;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D Corridors.
/// </summary>
public class Param_CivilCorridor : Param_AutocadObjectBase<GH_CivilCorridor, CivilCorridor>
{
    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("33A15D0A-6665-4D71-B3CA-9EF9DD6D1E8E");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Civil.GrasshopperLibrary.Properties.Resources.Param_CivilCorridor;

    /// <inheritdoc />
    protected override string SingularPromptMessage => "Select a Civil3d Corridor";

    /// <inheritdoc />
    protected override string PluralPromptMessage => "Select Civil3d Corridors";

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilCorridor"/> class.
    /// </summary>
    public Param_CivilCorridor()
        : base("Civil3d Corridor", "CVL-Corr",
            "A Civil 3D Corridor", "Params", "Civil3d")
    { }

    /// <inheritdoc />
    protected override IObjectFilter CreateSelectionFilter() => new CivilCorridorFilter();

    /// <inheritdoc />
    protected override GH_CivilCorridor WrapEntity(CivilCorridor entity)
    {
        return new GH_CivilCorridor(entity);
    }
}
