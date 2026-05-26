using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A value list component that provides a dropdown of Civil 3D subassembly sides.
/// </summary>
public class CivilSideValueList : GH_ValueList
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("B4C5D6E7-F8A9-0B1C-2D3E-4F5A6B7C8D9E");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.tertiary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilSideValueList;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilSideValueList"/> class.
    /// </summary>
    public CivilSideValueList()
    {
        this.Name = "Civil3d Side";
        this.NickName = "CVL-Side";
        this.Description = "Select a Civil 3D assembly side";
        this.Category = "Civil3d";
        this.SubCategory = "Assemblies";

        this.ListItems.Clear();

        this.ListItems.Add(new GH_ValueListItem("None", "0"));
        this.ListItems.Add(new GH_ValueListItem("Left", "1"));
        this.ListItems.Add(new GH_ValueListItem("Right", "2"));

        this.SelectItem(0);
    }
}
