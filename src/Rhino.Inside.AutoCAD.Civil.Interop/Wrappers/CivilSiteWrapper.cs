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
public class CivilSiteWrapper : ICivilSite
{
    /// <inheritdoc />
    public IObjectId Id { get; }

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
    /// <param name="transactionManager">The transaction manager for database access.</param>
    public CivilSiteWrapper(Site site, IAutocadTransactionManager transactionManager)
    {
        Id = new AutocadObjectIdWrapper(site.ObjectId);
        Name = site.Name;
        Description = site.Description ?? string.Empty;

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
        ParcelIds = parcelIds;
        ParcelCount = parcelIds.Count;

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
        AlignmentIds = alignmentIds;
        AlignmentCount = alignmentIds.Count;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilSiteWrapper"/>
    /// with explicit values.
    /// </summary>
    public CivilSiteWrapper(
        IObjectId id,
        string name,
        string description,
        IReadOnlyList<IObjectId> parcelIds,
        IReadOnlyList<IObjectId> alignmentIds)
    {
        Id = id;
        Name = name;
        Description = description;
        ParcelIds = parcelIds;
        ParcelCount = parcelIds.Count;
        AlignmentIds = alignmentIds;
        AlignmentCount = alignmentIds.Count;
    }

    /// <summary>
    /// Creates a duplicate of this site wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilSiteWrapper Duplicate()
    {
        return new CivilSiteWrapper(
            Id,
            Name,
            Description,
            ParcelIds.ToList(),
            AlignmentIds.ToList());
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Site: {Name} (Parcels: {ParcelCount}, Alignments: {AlignmentCount})";
    }
}
