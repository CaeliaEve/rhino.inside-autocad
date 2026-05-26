using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices.Styles;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <inheritdoc cref="ICivilAlignmentLabelSetStyle"/>
/// <remarks>
/// Wraps an AutoCAD Civil 3D <see cref="AlignmentLabelSetStyle"/> to expose style properties.
/// Used by Grasshopper components to specify label set styles when creating alignments.
/// </remarks>
public class CivilAlignmentLabelSetStyleWrapper : AutocadDbObjectWrapper, ICivilAlignmentLabelSetStyle
{
    private readonly AlignmentLabelSetStyle _labelSetStyle;

    /// <inheritdoc/>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilAlignmentLabelSetStyleWrapper"/>.
    /// </summary>
    /// <param name="labelSetStyle">
    /// The Civil 3D <see cref="AlignmentLabelSetStyle"/> to wrap.
    /// </param>
    public CivilAlignmentLabelSetStyleWrapper(AlignmentLabelSetStyle labelSetStyle) : base(labelSetStyle)
    {
        _labelSetStyle = labelSetStyle;
        this.Name = labelSetStyle.Name;
    }

    /// <inheritdoc/>
    public override IDbObject ShallowClone()
    {
        return new CivilAlignmentLabelSetStyleWrapper(_labelSetStyle);
    }

    /// <summary>
    /// Gets all alignment label set style names from the active Civil 3D document.
    /// </summary>
    /// <param name="database">The database to search.</param>
    /// <returns>A list of all alignment label set style names.</returns>
    public static IReadOnlyList<string> GetAllStyleNames(Database database)
    {
        var civilDoc = CivilApplication.ActiveDocument;
        var styles = civilDoc.Styles.LabelSetStyles.AlignmentLabelSetStyles;
        var names = new List<string>();

        foreach (var styleId in styles)
        {
            if (styleId.IsValid && !styleId.IsNull && !styleId.IsErased)
            {
                using var transaction = database.TransactionManager.StartTransaction();
                var style = transaction.GetObject(styleId, OpenMode.ForRead) as AlignmentLabelSetStyle;
                if (style != null)
                {
                    names.Add(style.Name);
                }
                transaction.Commit();
            }
        }

        return names;
    }
}
