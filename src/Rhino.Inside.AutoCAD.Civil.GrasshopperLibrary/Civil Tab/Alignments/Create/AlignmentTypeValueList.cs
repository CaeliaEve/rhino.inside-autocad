using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A value list component that provides a dropdown of Civil 3D alignment types.
/// </summary>
public class AlignmentTypeValueList : GH_ValueList
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("A3B4C5D6-E7F8-9A0B-1C2D-3E4F5A6B7C8D");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.tertiary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.AlignmentTypeValueList;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlignmentTypeValueList"/> class.
    /// </summary>
    public AlignmentTypeValueList()
    {
        this.Name = "Civil3d Alignment Type";
        this.NickName = "CVL-AlnType";
        this.Description = "Select a Civil 3D alignment type";
        this.Category = "Civil3d";
        this.SubCategory = "Alignments";

        this.ListItems.Clear();

        this.ListItems.Add(new GH_ValueListItem("Centerline", "1"));
        this.ListItems.Add(new GH_ValueListItem("Offset", "2"));
        this.ListItems.Add(new GH_ValueListItem("Curb Return", "3"));
        this.ListItems.Add(new GH_ValueListItem("Utility", "4"));
        this.ListItems.Add(new GH_ValueListItem("Rail", "5"));

        this.SelectItem(0);
    }
}
