using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
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
public class CivilParcelPropertiesWrapper : ICivilParcelProperties
{
    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string Description { get; }

    /// <inheritdoc />
    public double Area { get; }

    /// <inheritdoc />
    public double Perimeter { get; }

    /// <inheritdoc />
    public int Number { get; }

    /// <inheritdoc />
    public int TaxId { get; }

    /// <inheritdoc />
    public string Address { get; }

    /// <inheritdoc />
    public string SiteName { get; }

    /// <inheritdoc />
    public int SegmentCount { get; }

    /// <inheritdoc />
    public bool IsClosed { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilParcelPropertiesWrapper"/>
    /// from a Civil 3D Parcel.
    /// </summary>
    /// <param name="parcel">The parcel to extract properties from.</param>
    public CivilParcelPropertiesWrapper(Parcel parcel)
    {
        this.Name = parcel.Name;
        this.Description = parcel.Description ?? string.Empty;
        this.Area = parcel.Area;
        this.Number = parcel.Number;
        this.TaxId = parcel.TaxId;
        this.Address = parcel.Address ?? string.Empty;

        // Derive properties from BaseCurve that aren't directly available on Parcel
        var baseCurve = parcel.BaseCurve;
        this.Perimeter = GetPerimeterFromBaseCurve(baseCurve);
        this.IsClosed = GetIsClosedFromBaseCurve(baseCurve);
        this.SegmentCount = CountSegmentsFromBaseCurve(baseCurve);

        // SiteName requires accessing the Site object - get from parcel's site if available
        this.SiteName = GetSiteNameFromParcel(parcel);
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

    /// <summary>
    /// Initializes a new instance of <see cref="CivilParcelPropertiesWrapper"/>
    /// with explicit values.
    /// </summary>
    public CivilParcelPropertiesWrapper(
        string name,
        string description,
        double area,
        double perimeter,
        int number,
        int taxId,
        string address,
        string siteName,
        int segmentCount,
        bool isClosed)
    {
        this.Name = name;
        this.Description = description;
        this.Area = area;
        this.Perimeter = perimeter;
        this.Number = number;
        this.TaxId = taxId;
        this.Address = address;
        this.SiteName = siteName;
        this.SegmentCount = segmentCount;
        this.IsClosed = isClosed;
    }

    /// <summary>
    /// Creates a duplicate of this parcel properties wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilParcelPropertiesWrapper Duplicate()
    {
        return new CivilParcelPropertiesWrapper(
            this.Name,
            this.Description,
            this.Area,
            this.Perimeter,
            this.Number,
            this.TaxId,
            this.Address,
            this.SiteName,
            this.SegmentCount,
            this.IsClosed);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Parcel Properties: {this.Name} (Area: {this.Area:F2}, Perimeter: {this.Perimeter:F2})";
    }
}
