using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;
using DBObject = Autodesk.AutoCAD.DatabaseServices.DBObject;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts individual values from a Civil 3D Profile Label Group.
/// </summary>
/// <remarks>
/// This component works with profile label groups and exposes their common properties.
/// </remarks>
[ComponentVersion(introduced: "1.0.19")]
public class CivilProfileLabelGroupComponent : RhinoInsideAutocad_ComponentBase
{
    private readonly GooTypeRegistry _gooConverterRegister = GooTypeRegistry.Instance!;

    /// <inheritdoc />
    public override Guid ComponentGuid => new("C3D4E5F6-A7B8-9012-CDEF-345678901234");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilProfileLabelGroupComponent"/> class.
    /// </summary>
    public CivilProfileLabelGroupComponent()
        : base("Civil3d Profile Label Group", "CVL-ProfLblGrp",
            "Extracts individual values from a Civil 3D Profile Label Group",
            "Civil3d", "Labels")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilProfileLabelGroup(GH_ParamAccess.item), "Label Group",
            "LG", "A profile label group from a Civil3d Profile", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Label Group Type", "Type",
            "The type of this label group (e.g., ProfileLabelGroup).", GH_ParamAccess.item);

        pManager.AddTextParameter("Style Name", "Style",
            "The name of the label style applied to this group.", GH_ParamAccess.item);

        pManager.AddIntegerParameter("Label Count", "Count",
            "The number of sub-entity labels in this group.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Range Start", "RStart",
            "The start station of the label range.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Range End", "REnd",
            "The end station of the label range.", GH_ParamAccess.item);

        pManager.AddBooleanParameter("Range Start From Feature", "RSFeat",
            "Whether the start of the range is derived from the profile feature.", GH_ParamAccess.item);

        pManager.AddBooleanParameter("Range End From Feature", "REFeat",
            "Whether the end of the range is derived from the profile feature.", GH_ParamAccess.item);

        pManager.AddBooleanParameter("Is Visible", "Vis",
            "The visibility state of the label group.", GH_ParamAccess.item);

        pManager.AddGeometryParameter("Geometric Entities", "Entities",
            "The geometric entities in the label group, organized by label (one branch per label)", GH_ParamAccess.tree);
    }

    private List<IDbObject> RemoveBlocks(List<IDbObject> dbObjects,
        IAutocadTransactionManager transactionManager)
    {
        var listEntities = new List<IDbObject>();

        foreach (var dbObject in dbObjects)
        {
            if (dbObject.UnwrapObject() is BlockReference blockReference == false)
            {
                listEntities.Add(dbObject);
                continue;
            }

            var referenceWrapper = new AutocadBlockReferenceWrapper(blockReference);

            var explodedEntities = referenceWrapper.GetObjects(transactionManager);

            var castDown = explodedEntities.Cast<IDbObject>().ToList();

            listEntities.AddRange(this.RemoveBlocks(castDown, transactionManager));
        }

        return listEntities;
    }

    /// <summary>
    /// Explodes the label group per sub-entity using visibility swapping.
    /// Each sub-entity is made visible one at a time while others are hidden,
    /// then the group is exploded to capture only that label's geometry.
    /// </summary>
    /// <param name="labelGroup">The profile label group to explode.</param>
    /// <param name="transactionManager">The transaction manager for database access.</param>
    /// <returns>A DataTree where each branch contains geometry for a single label.</returns>
    private GH_Structure<IGH_GeometricGoo> ExplodePerSubEntity(
        ProfileLabelGroup labelGroup,
        IAutocadTransactionManager transactionManager)
    {
        var tree = new GH_Structure<IGH_GeometricGoo>();
        var subEntityCount = (int)labelGroup.SubEntityCount;

        if (subEntityCount == 0)
        {
            return tree;
        }

        // Store original visibility states for all sub-entities
        var originalVisibility = new bool[subEntityCount];
        for (var i = 0; i < subEntityCount; i++)
        {
            originalVisibility[i] = labelGroup.GetAt((uint)i).Visibility;
        }

        try
        {
            for (var i = 0; i < subEntityCount; i++)
            {
                // Hide all sub-entities
                for (var j = 0; j < subEntityCount; j++)
                {
                    labelGroup.GetAt((uint)j).Visibility = false;
                }

                // Make only this sub-entity visible
                labelGroup.GetAt((uint)i).Visibility = true;

                // Explode and capture geometry for this label only
                var objectCollection = new DBObjectCollection();
                labelGroup.Explode(objectCollection);

                var listEntities = new List<IDbObject>();
                foreach (DBObject dbObject in objectCollection)
                {
                    listEntities.Add(new AutocadDbObjectWrapper(dbObject));
                }

                var blocklessEntities = this.RemoveBlocks(listEntities, transactionManager);

                // Add to DataTree at branch [i]
                var path = new GH_Path(i);
                foreach (var dbObject in blocklessEntities)
                {
                    if (dbObject is not IEntity entity) continue;
                    var goo = _gooConverterRegister.CreateGeometryGoo(entity);
                    if (goo != null)
                    {
                        tree.Append(goo, path);
                    }
                }
            }
        }
        catch
        {
            // If visibility manipulation fails, fall back to single explode
            // Restore visibility first
            for (var i = 0; i < subEntityCount; i++)
            {
                try
                {
                    labelGroup.GetAt((uint)i).Visibility = originalVisibility[i];
                }
                catch
                {
                    // Ignore restore errors
                }
            }
            return this.ExplodeFallback(labelGroup, transactionManager);
        }
        finally
        {
            // Restore all original visibility states
            for (var i = 0; i < subEntityCount; i++)
            {
                try
                {
                    labelGroup.GetAt((uint)i).Visibility = originalVisibility[i];
                }
                catch
                {
                    // Ignore restore errors
                }
            }
        }

        return tree;
    }

    /// <summary>
    /// Fallback method that explodes all geometry into a single branch.
    /// Used when visibility style manipulation fails.
    /// </summary>
    private GH_Structure<IGH_GeometricGoo> ExplodeFallback(
        LabelGroup labelGroup,
        IAutocadTransactionManager transactionManager)
    {
        var tree = new GH_Structure<IGH_GeometricGoo>();

        var objectCollection = new DBObjectCollection();
        labelGroup.Explode(objectCollection);

        var listEntities = new List<IDbObject>();
        foreach (DBObject dbObject in objectCollection)
        {
            listEntities.Add(new AutocadDbObjectWrapper(dbObject));
        }

        var blocklessEntities = this.RemoveBlocks(listEntities, transactionManager);

        var path = new GH_Path(0);
        foreach (var dbObject in blocklessEntities)
        {
            if (dbObject is not IEntity entity) continue;
            var goo = _gooConverterRegister.CreateGeometryGoo(entity);
            if (goo != null)
            {
                tree.Append(goo, path);
            }
        }

        return tree;
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilProfileLabelGroup? labelGroupGoo = null;

        if (!DA.GetData(0, ref labelGroupGoo) || labelGroupGoo?.Value is null) return;

        var labelGroup = labelGroupGoo.Value;

        DA.SetData(0, labelGroup.LabelGroupType);
        DA.SetData(1, labelGroup.StyleName);
        DA.SetData(2, labelGroup.LabelCount);
        DA.SetData(3, labelGroup.RangeStart);
        DA.SetData(4, labelGroup.RangeEnd);
        DA.SetData(5, labelGroup.RangeStartFromFeature);
        DA.SetData(6, labelGroup.RangeEndFromFeature);
        DA.SetData(7, labelGroup.IsVisible);

        var cadLabelGroup = labelGroup.Unwrap();

        if (cadLabelGroup == null) return;

        var document = RhinoInsideAutoCadExtension.Application.RhinoInsideManager
            .AutoCadInstance.ActiveDocument;

        var transactionManager = document.CreateTransactionManager();

        // Use visibility swapping approach to extract geometry per sub-entity
        var geometryTree = transactionManager.PerformTask(() =>
        {
            var profileLabelGroup = (ProfileLabelGroup)transactionManager.Unwrap()
                .GetObject(cadLabelGroup.Id, OpenMode.ForWrite);

            return this.ExplodePerSubEntity(profileLabelGroup, transactionManager);
        }, true);

        DA.SetDataTree(8, geometryTree);
    }
}
