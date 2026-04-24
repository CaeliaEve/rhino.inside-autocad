using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps a Civil 3D Site container.
/// </summary>
/// <remarks>
/// This is a data wrapper class that holds extracted site information.
/// The data is captured at construction time from a <see cref="Site"/>.
/// </remarks>
public class CivilSiteWrapper : AutocadEntityWrapper, ICivilSite
{
    private readonly Site _site;

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string Description { get; }

    /// <inheritdoc />
    public int ParcelCount { get; }

    /// <inheritdoc />
    public int AlignmentCount { get; }

    /// <inheritdoc />
    public IReadOnlyList<IObjectId> ParcelIds { get; }

    /// <inheritdoc />
    public IReadOnlyList<IObjectId> AlignmentIds { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilSiteWrapper"/>
    /// from a Civil 3D Site.
    /// </summary>
    /// <param name="site">The site to extract properties from.</param>
    public CivilSiteWrapper(Site site) : base(site)
    {
        _site = site;
        this.Name = site.Name;
        this.Description = site.Description ?? string.Empty;

        // Get parcel IDs
        var parcelIds = new List<IObjectId>();
        try
        {
            var parcelObjectIds = site.GetParcelIds();
            foreach (ObjectId parcelId in parcelObjectIds)
            {
                if (!parcelId.IsNull && !parcelId.IsErased)
                {
                    parcelIds.Add(new AutocadObjectIdWrapper(parcelId));
                }
            }
        }
        catch
        {
            // Parcels may not be available
        }
        this.ParcelIds = parcelIds;
        this.ParcelCount = parcelIds.Count;

        // Get alignment IDs
        var alignmentIds = new List<IObjectId>();
        try
        {
            var alignmentObjectIds = site.GetAlignmentIds();
            foreach (ObjectId alignmentId in alignmentObjectIds)
            {
                if (!alignmentId.IsNull && !alignmentId.IsErased)
                {
                    alignmentIds.Add(new AutocadObjectIdWrapper(alignmentId));
                }
            }
        }
        catch
        {
            // Alignments may not be available
        }
        this.AlignmentIds = alignmentIds;
        this.AlignmentCount = alignmentIds.Count;
    }

    /// <summary>
    /// Creates a duplicate of this site wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilSiteWrapper ShallowClone()
    {
        return new CivilSiteWrapper(_site);
    }

    /// <inheritdoc />
    public ICivilSite Update(IAutocadTransactionManager transactionManager,
        string newName, string newDescription)
    {
        var site = transactionManager.Unwrap()
            .GetObject(_site.Id, OpenMode.ForWrite) as Site;

        if (site == null)
        {
            return this;
        }

        site.Name = newName;
        site.Description = newDescription;

        return new CivilSiteWrapper(site);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Site: {this.Name} (Parcels: {this.ParcelCount}, Alignments: {this.AlignmentCount})";
    }
}
