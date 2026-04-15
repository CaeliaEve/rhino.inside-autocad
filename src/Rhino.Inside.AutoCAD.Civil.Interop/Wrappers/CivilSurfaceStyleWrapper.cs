using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices.Styles;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <inheritdoc cref="ICivilSurfaceStyle"/>
/// <remarks>
/// Wraps an AutoCAD Civil 3D <see cref="SurfaceStyle"/> to expose style properties.
/// Used by Grasshopper components to specify surface styles when creating TIN surfaces.
/// </remarks>
public class CivilSurfaceStyleWrapper : AutocadDbObjectWrapper, ICivilSurfaceStyle
{
    private readonly SurfaceStyle _surfaceStyle;

    /// <inheritdoc/>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilSurfaceStyleWrapper"/>.
    /// </summary>
    /// <param name="surfaceStyle">
    /// The Civil 3D <see cref="SurfaceStyle"/> to wrap.
    /// </param>
    public CivilSurfaceStyleWrapper(SurfaceStyle surfaceStyle) : base(surfaceStyle)
    {
        _surfaceStyle = surfaceStyle;
        this.Name = surfaceStyle.Name;
    }

    /// <inheritdoc/>
    public override IDbObject ShallowClone()
    {
        return new CivilSurfaceStyleWrapper(_surfaceStyle);
    }

    /// <summary>
    /// Gets all surface style names from the specified database.
    /// </summary>
    /// <param name="database">The database to search.</param>
    /// <returns>A list of all surface style names.</returns>
    public static IReadOnlyList<string> GetAllStyleNames(Database database)
    {
        var civilDoc = CivilApplication.ActiveDocument;
        var styles = civilDoc.Styles.SurfaceStyles;
        var names = new List<string>();

        foreach (var styleId in styles)
        {
            if (styleId.IsValid && !styleId.IsNull && !styleId.IsErased)
            {
                using var transaction = database.TransactionManager.StartTransaction();
                var style = transaction.GetObject(styleId, OpenMode.ForRead) as SurfaceStyle;
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
