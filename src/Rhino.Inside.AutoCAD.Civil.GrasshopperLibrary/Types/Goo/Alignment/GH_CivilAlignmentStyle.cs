using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices.Styles;
using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D alignment styles.
/// </summary>
public class GH_CivilAlignmentStyle : GH_AutocadObjectGoo<CivilAlignmentStyleWrapper>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignmentStyle"/> class with no value.
    /// </summary>
    public GH_CivilAlignmentStyle()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignmentStyle"/> class with the
    /// specified Civil 3D alignment style.
    /// </summary>
    /// <param name="styleWrapper">The Civil 3D alignment style to wrap.</param>
    public GH_CivilAlignmentStyle(CivilAlignmentStyleWrapper styleWrapper) : base(styleWrapper)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignmentStyle"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilAlignmentStyle(GH_CivilAlignmentStyle other)
    {
        this.Value = other.Value;
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilAlignmentStyle"/> via the interface.
    /// </summary>
    public GH_CivilAlignmentStyle(ICivilAlignmentStyle alignmentStyle)
        : base((alignmentStyle as CivilAlignmentStyleWrapper)!)
    {
    }

    /// <inheritdoc />
    protected override Type GetCadType() => typeof(AlignmentStyle);

    /// <inheritdoc />
    protected override IGH_Goo CreateInstance(IDbObject dbObject)
    {
        var unwrapped = dbObject.UnwrapObject();

        var newWrapper = new CivilAlignmentStyleWrapper(unwrapped as AlignmentStyle);

        return new GH_CivilAlignmentStyle(newWrapper);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        // Handle string input - resolve style name to ObjectId
        if (source is string styleName)
        {
            return this.TryResolveStyleByName(styleName);
        }

        if (source is GH_String ghString)
        {
            return this.TryResolveStyleByName(ghString.Value);
        }

        // Handle ObjectId input
        if (source is GH_AutocadObjectId ghObjectId)
        {
            return this.TryResolveStyleFromObjectId(ghObjectId.Value);
        }

        return base.CastFrom(source);
    }

    /// <summary>
    /// Attempts to resolve an alignment style by name from the active document.
    /// </summary>
    private bool TryResolveStyleByName(string styleName)
    {
        if (string.IsNullOrWhiteSpace(styleName))
            return false;

        var activeDoc = RhinoInsideAutoCadExtension.Application?.RhinoInsideManager?
            .AutoCadInstance?.ActiveDocument;

        if (activeDoc == null)
            return false;

        var transactionManager = activeDoc.CreateTransactionManager();

        var result = transactionManager.PerformTask(() =>
        {
            var transaction = transactionManager.Unwrap();

            var styles = CivilApplication.ActiveDocument.Styles.AlignmentStyles;

            if (!styles.Contains(styleName))
                return null;

            var styleId = styles[styleName];

            if (!styleId.IsValid || styleId.IsNull)
                return null;

            var style = transaction.GetObject(styleId, OpenMode.ForRead) as AlignmentStyle;
            return style != null ? new CivilAlignmentStyleWrapper(style) : null;
        });

        if (result != null)
        {
            this.Value = result;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Attempts to resolve an alignment style from an ObjectId wrapper.
    /// </summary>
    private bool TryResolveStyleFromObjectId(IObjectId objectId)
    {
        if (!objectId.IsValid)
            return false;

        var activeDoc = RhinoInsideAutoCadExtension.Application?.RhinoInsideManager?
            .AutoCadInstance?.ActiveDocument;

        if (activeDoc == null)
            return false;

        var transactionManager = activeDoc.CreateTransactionManager();

        var result = transactionManager.PerformTask(() =>
        {
            if (objectId.IsValid == false)
                return null;

            var transaction = transactionManager.Unwrap();

            var style = transaction.GetObject(objectId.Unwrap(), OpenMode.ForRead) as AlignmentStyle;

            return style != null ? new CivilAlignmentStyleWrapper(style) : null;
        });

        if (result != null)
        {
            this.Value = result;
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (this.Value == null)
            return "Null Civil3d Alignment Style";

        return $"Civil3d Alignment Style [{this.Value.Name}]";
    }
}
