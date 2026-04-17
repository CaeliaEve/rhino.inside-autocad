using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using CivilParcel = Autodesk.Civil.DatabaseServices.Parcel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D Parcels.
/// </summary>
public class Param_CivilParcel : Param_AutocadObjectBase<GH_CivilParcel, CivilParcel>
{
    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("B2C3D4E5-F6A7-8901-BCDE-F23456789012");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <inheritdoc />
    protected override string SingularPromptMessage => "Select a Civil3d Parcel";

    /// <inheritdoc />
    protected override string PluralPromptMessage => "Select Civil3d Parcels";

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilParcel"/> class.
    /// </summary>
    public Param_CivilParcel()
        : base("Civil3d Parcel", "CVL-Parcel",
            "A Civil 3D Parcel", "Params", "Civil3d")
    { }

    /// <inheritdoc />
    protected override IObjectFilter CreateSelectionFilter() => new CivilParcelFilter();

    /// <inheritdoc />
    protected override GH_CivilParcel WrapEntity(CivilParcel entity)
    {
        return new GH_CivilParcel(entity);
    }
}
