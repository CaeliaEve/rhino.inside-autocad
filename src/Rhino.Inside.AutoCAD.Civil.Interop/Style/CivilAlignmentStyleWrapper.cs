using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices.Styles;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <inheritdoc cref="ICivilAlignmentStyle"/>
/// <remarks>
/// Wraps an AutoCAD Civil 3D <see cref="AlignmentStyle"/> to expose style properties.
/// Used by Grasshopper components to specify alignment styles when creating alignments.
/// </remarks>
public class CivilAlignmentStyleWrapper : AutocadDbObjectWrapper, ICivilAlignmentStyle
{
    private readonly AlignmentStyle _alignmentStyle;

    /// <inheritdoc/>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilAlignmentStyleWrapper"/>.
    /// </summary>
    /// <param name="alignmentStyle">
    /// The Civil 3D <see cref="AlignmentStyle"/> to wrap.
    /// </param>
    public CivilAlignmentStyleWrapper(AlignmentStyle alignmentStyle) : base(alignmentStyle)
    {
        _alignmentStyle = alignmentStyle;
        this.Name = alignmentStyle.Name;
    }

    /// <inheritdoc/>
    public override IDbObject ShallowClone()
    {
        return new CivilAlignmentStyleWrapper(_alignmentStyle);
    }

    /// <summary>
    /// Gets all alignment style names from the active Civil 3D document.
    /// </summary>
    /// <param name="database">The database to search.</param>
    /// <returns>A list of all alignment style names.</returns>
    public static IReadOnlyList<string> GetAllStyleNames(Database database)
    {
        var civilDoc = CivilApplication.ActiveDocument;
        var styles = civilDoc.Styles.AlignmentStyles;
        var names = new List<string>();

        foreach (var styleId in styles)
        {
            if (styleId.IsValid && !styleId.IsNull && !styleId.IsErased)
            {
                using var transaction = database.TransactionManager.StartTransaction();
                var style = transaction.GetObject(styleId, OpenMode.ForRead) as AlignmentStyle;
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
