using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using CadCurve = Autodesk.AutoCAD.DatabaseServices.Curve;
using RhinoCurve = Rhino.Geometry.Curve;
using RhinoPolyCurve = Rhino.Geometry.PolyCurve;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <inheritdoc cref="ICivilTinSurface"/>
public class CivilTinSurfaceWrapper : AutocadEntityWrapper, ICivilTinSurfaceWrapper
{
    private readonly TinSurface _surface;

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public ICivilTinSurfaceProperties Properties { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilTinVolumeSurfaceWrapper"/>.
    /// </summary>
    /// <param name="surface">
    /// The Civil 3D <see cref="TinSurface"/> to wrap.
    /// </param>
    public CivilTinSurfaceWrapper(TinSurface surface) : base(surface)
    {
        _surface = surface;
        this.Name = surface.Name;
        this.Properties = new CivilTinSurfaceProperties(surface);
    }

    /// <summary>
    /// Extracts contours of a specific type from the surface.
    /// </summary>
    private List<ICivilSurfaceContour> ExtractContoursOfType(
        IAutocadTransactionManager transaction,
        ObjectIdCollection contourIds,
        CivilContourType civilContourType)
    {
        var contours = new List<ICivilSurfaceContour>();

        try
        {

            if (contourIds == null || contourIds.Count == 0)
                return [];

            foreach (ObjectId id in contourIds)
            {
                var entity = transaction.Unwrap().GetObject(id, OpenMode.ForRead);

                RhinoCurve? rhinoCurve = null;
                var elevation = 0.0;

                switch (entity)
                {
                    case Polyline3d polyline3d:
                        rhinoCurve = polyline3d.ToRhinoCurve(transaction);
                        break;
                    case Polyline polyline:
                        rhinoCurve = polyline.ToRhinoCurve();
                        break;
                    case CadCurve curve:
                        rhinoCurve = curve.ToRhinoCurve();
                        break;
                }

                if (rhinoCurve != null)
                {
                    contours.Add(new CivilSurfaceContour(civilContourType, rhinoCurve));
                }

                // Erase the temporary extracted entity
                entity.UpgradeOpen();
                entity.Erase();
            }
        }
        catch
        {
            // If extraction fails for this type, continue with other types
        }

        return contours;
    }

    /// <summary>
    /// Extracts the outer border of the surface.
    /// </summary>
    private RhinoPolyCurve? ExtractOuterBorder(IAutocadTransactionManager transactionWrapper)
    {
        var borderIds = _surface.ExtractBorder(SurfaceExtractionSettingsType.Plan);

        if (borderIds == null || borderIds.Count == 0)
            return null;

        var transaction = transactionWrapper.Unwrap();

        var polyCurve = new RhinoPolyCurve();

        foreach (ObjectId id in borderIds)
        {
            var cadCurve = transaction.GetObject(id, OpenMode.ForRead) as CadCurve;
            var rhinoCurve = cadCurve.ToRhinoCurve();

            polyCurve.Append(rhinoCurve);
        }
        if (polyCurve.IsClosed == false)
        {
            polyCurve.MakeClosed(GeometryConstants.ShortCurveTolernace);
        }
        return polyCurve;

    }

    /// <inheritdoc/>
    public override IDbObject ShallowClone()
    {
        return new CivilTinSurfaceWrapper(_surface);
    }

    /// <inheritdoc />
    public IReadOnlyList<ICivilSurfaceContour> GetContours(IAutocadTransactionManager transaction)
    {
        var contours = new List<ICivilSurfaceContour>();

        var transactionManager = transaction.Unwrap();

        var surface = transactionManager.GetObject(_surface.Id, OpenMode.ForWrite) as TinSurface;

        if (surface == null)
            return contours;

        var majorContours = surface.ExtractMajorContours(SurfaceExtractionSettingsType.Model);
        contours.AddRange(this.ExtractContoursOfType(transaction, majorContours, CivilContourType.Major));

        var minorContours = surface.ExtractMinorContours(SurfaceExtractionSettingsType.Model);
        contours.AddRange(this.ExtractContoursOfType(transaction, minorContours, CivilContourType.Minor));

        return contours;
    }

    /// <inheritdoc />
    public IReadOnlyList<ICivilSurfaceBreakline> GetBreaklines(IAutocadTransactionManager transaction)
    {
        var breaklines = new List<ICivilSurfaceBreakline>();

        var surface = transaction.Unwrap()
            .GetObject(_surface.Id, OpenMode.ForRead) as TinSurface;

        if (surface == null)
            return breaklines;

        var breaklinesDefinition = surface.BreaklinesDefinition;

        for (var i = 0; i < breaklinesDefinition.Count; i++)
        {
            var breaklineDefinition = breaklinesDefinition[i];

            var breaklineType = breaklineDefinition.BreaklineType.ToRhinoInsideBreaklineType();

            var definitionDescription = breaklineDefinition.Description ?? $"Breakline_{i}";

            for (var j = 0; j < breaklineDefinition.Count; j++)
            {
                var breaklineData = breaklineDefinition[j];

                var curve = breaklineData.ToRhinoCurve();

                if (curve == null) continue;

                var name = breaklineDefinition.Count > 1 ? $"{definitionDescription}_{j}" : definitionDescription;

                breaklines.Add(new CivilSurfaceBreakline(breaklineType, curve, name));
            }
        }

        return breaklines;
    }

    /// <inheritdoc />
    public IReadOnlyList<ICivilSurfaceBoundary> GetBoundaries(IAutocadTransactionManager transaction)
    {
        var boundaries = new List<ICivilSurfaceBoundary>();

        var surface = transaction.Unwrap()
            .GetObject(_surface.Id, OpenMode.ForWrite) as TinSurface;

        var outerBorder = this.ExtractOuterBorder(transaction);
        if (outerBorder != null)
        {
            boundaries.Add(new CivilSurfaceBoundary(Core.CivilSurfaceBoundaryType.Outer, outerBorder, "Outer Border"));
        }

        var boundariesDefinition = surface.BoundariesDefinition;

        for (var i = 0; i < boundariesDefinition.Count; i++)
        {
            var boundaryDefinition = boundariesDefinition[i];

            var boundaryType = boundaryDefinition.BoundaryType.ToRhinoInsideBoundaryType();

            var definitionName = boundaryDefinition.Name ?? $"BoundaryOp_{i}";

            for (var j = 0; j < boundaryDefinition.Count; j++)
            {
                var surfaceBoundary = boundaryDefinition[j];

                var polyline = surfaceBoundary.ToRhinoCurve();

                var name = boundaryDefinition.Count > 1 ? $"{definitionName}_{j}" : definitionName;

                boundaries.Add(new CivilSurfaceBoundary(boundaryType, polyline, name));

            }
        }

        return boundaries;
    }
}
