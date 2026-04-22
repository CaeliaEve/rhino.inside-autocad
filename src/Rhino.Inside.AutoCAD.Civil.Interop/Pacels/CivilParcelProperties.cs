using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using CadPolyline = Autodesk.AutoCAD.DatabaseServices.Polyline;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps properties extracted from a Civil 3D Parcel.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted parcel property information.
/// The data is captured at construction time from a <see cref="Parcel"/>.
/// Some properties are derived from the BaseCurve geometry when not directly available.
/// </remarks>
public record CivilParcelProperties : ICivilParcelProperties
{
    /// <summary>
    /// Constructs a new instance of <see cref="CivilParcelProperties"/> by extracting
    /// data from a given <see cref="Parcel"/>.
    /// </summary>
    public static CivilParcelProperties CreateFromParcel(Parcel parcel)
    {
        var baseCurve = parcel.BaseCurve;

        return new CivilParcelProperties()
        {
            Name = parcel.Name,
            Description = parcel.Description ?? string.Empty,
            Area = parcel.Area,
            Number = parcel.Number,
            TaxId = parcel.TaxId,
            Address = parcel.Address ?? string.Empty,
            Perimeter = GetPerimeterFromBaseCurve(baseCurve),
            IsClosed = GetIsClosedFromBaseCurve(baseCurve),
            SegmentCount = CountSegmentsFromBaseCurve(baseCurve),
            SiteName = GetSiteNameFromParcel(parcel),
            Style = new NamedId(parcel.StyleName, parcel.StyleId),
        };
    }

    /// <inheritdoc />
    public string Name { get; init; } = string.Empty;

    /// <inheritdoc />
    public string Description { get; init; } = string.Empty;

    /// <inheritdoc />
    public double Area { get; init; }

    /// <inheritdoc />
    public double Perimeter { get; init; }

    /// <inheritdoc />
    public int Number { get; init; }

    /// <inheritdoc />
    public int TaxId { get; init; }

    /// <inheritdoc />
    public string Address { get; init; } = string.Empty;

    /// <inheritdoc />
    public string SiteName { get; init; } = string.Empty;

    /// <inheritdoc />
    public int SegmentCount { get; init; }

    /// <inheritdoc />
    public bool IsClosed { get; init; }

    /// <inheritdoc />
    public INamedId Style { get; init; } = NamedId.Empty;

    /// <summary>
    /// Initializes a new private empty instance of <see cref="CivilParcelProperties"/>
    /// </summary>
    private CivilParcelProperties()
    {
    }

    /// <summary>
    /// Creates a duplicate of this parcel properties wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilParcelProperties Duplicate()
    {
        return new CivilParcelProperties()
        {
            Name = this.Name,
            Description = this.Description,
            Area = this.Area,
            Perimeter = this.Perimeter,
            Number = this.Number,
            TaxId = this.TaxId,
            Address = this.Address,
            SiteName = this.SiteName,
            SegmentCount = this.SegmentCount,
            IsClosed = this.IsClosed,
            Style = this.Style.ShallowClone() as NamedId ?? NamedId.Empty,
        };
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Parcel Properties: {this.Name} (Area: {this.Area:F2}, Perimeter: {this.Perimeter:F2})";
    }

    /// <summary>
    /// Gets the perimeter from the base curve.
    /// </summary>
    private static double GetPerimeterFromBaseCurve(Curve? baseCurve)
    {
        try
        {
            if (baseCurve == null)
                return 0.0;

            return baseCurve.GetDistanceAtParameter(baseCurve.EndParam);
        }
        catch
        {
            return 0.0;
        }
    }

    /// <summary>
    /// Determines if the parcel is closed from the base curve.
    /// </summary>
    private static bool GetIsClosedFromBaseCurve(Curve? baseCurve)
    {
        try
        {
            if (baseCurve == null)
                return false;

            return baseCurve.Closed;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the site name from the parcel.
    /// </summary>
    private static string GetSiteNameFromParcel(Parcel parcel)
    {
        try
        {
            var sitename = "";
            using (var tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                var aeccsite = parcel.AcadObject.GetType().InvokeMember("Parent", System.Reflection.BindingFlags.GetProperty, null, parcel.AcadObject, null);
                var site = tr.GetObject(Autodesk.Civil.DatabaseServices.DBObject.FromAcadObject(aeccsite), OpenMode.ForRead) as Site;
                sitename = site.Name;
                tr.Commit();
            }
            return sitename;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Counts segments from the parcel's base curve.
    /// </summary>
    private static int CountSegmentsFromBaseCurve(Curve? baseCurve)
    {
        try
        {
            if (baseCurve == null)
                return 0;

            // For polylines, count the number of vertices - 1 (or vertices for closed)
            if (baseCurve is CadPolyline polyline)
            {
                return polyline.NumberOfVertices > 0
                    ? (polyline.Closed ? polyline.NumberOfVertices : polyline.NumberOfVertices - 1)
                    : 0;
            }

            // For other curve types, return 1
            return 1;
        }
        catch
        {
            return 0;
        }
    }
}
