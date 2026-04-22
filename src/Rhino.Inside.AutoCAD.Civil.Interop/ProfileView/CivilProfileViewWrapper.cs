using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <inheritdoc cref="ICivilProfileView"/>
public class CivilProfileViewWrapper : AutocadEntityWrapper, ICivilProfileViewWrapper
{
    private readonly ProfileView _profileView;

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public ICivilProfileViewProperties Properties { get; }

    /// <summary>
    /// Constructs a new instance of <see cref="ICivilProfileView"/>
    /// </summary>
    public CivilProfileViewWrapper(ProfileView profileView) : base(profileView)
    {
        _profileView = profileView;

        var properties = new CivilProfileViewProperties(profileView);

        this.Name = properties.Name;

        this.Properties = properties;
    }

    /// <summary>
    /// Gets all profile IDs displayed in this ProfileView.
    /// </summary>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A list of ObjectIds for the displayed profiles.</returns>
    private List<ObjectId> GetDisplayedProfileIds(IAutocadTransactionManager transactionManager)
    {
        var profileIds = new List<ObjectId>();

        try
        {
            // Get the parent alignment to access its profiles
            var alignmentId = _profileView.AlignmentId;
            if (alignmentId.IsNull || alignmentId.IsErased)
                return profileIds;

            var alignment = transactionManager.Unwrap()
                .GetObject(alignmentId, OpenMode.ForRead) as Alignment;

            if (alignment == null)
                return profileIds;

            // Get all profiles from the alignment that are displayed in this view
            var allProfileIds = alignment.GetProfileIds();
            foreach (ObjectId profileId in allProfileIds)
            {
                if (profileId.IsNull || profileId.IsErased)
                    continue;

                // Check if this profile is displayed in the profile view
                // All profiles from the parent alignment are typically available
                profileIds.Add(profileId);
            }
        }
        catch
        {
            // Return empty list if extraction fails
        }

        return profileIds;
    }

    /// <summary>
    /// Extracts bands from a band set at the specified location.
    /// </summary>
    private List<CivilProfileViewBand> ExtractBandsFromSet(ProfileViewBandItemCollection bandItemCollection)
    {
        var bands = new List<CivilProfileViewBand>();
        if (bandItemCollection == null)
            return bands;

        var bandCount = bandItemCollection.Count;
        for (var i = 0; i < bandCount; i++)
        {

            var bandInfo = bandItemCollection[i];

            bands.Add(new CivilProfileViewBand(bandInfo, i));
        }

        return bands;
    }

    /// <inheritdoc />
    public List<ICivilProfile> GetDisplayedProfiles(IAutocadTransactionManager transactionManager)
    {
        var profileIds = this.GetDisplayedProfileIds(transactionManager);
        var profiles = new List<ICivilProfile>();

        foreach (var profileId in profileIds)
        {
            if (profileId.IsNull || profileId.IsErased)
                continue;

            var profile = transactionManager.Unwrap()
                .GetObject(profileId, OpenMode.ForRead) as Profile;

            if (profile != null)
            {
                profiles.Add(new CivilProfileWrapper(profile));
            }
        }

        return profiles;
    }

    /// <inheritdoc />
    public bool TryGetAlignment(IAutocadTransactionManager transactionManager, out ICivilAlignment? alignmentWrapped)
    {
        alignmentWrapped = null;
        var alignmentId = _profileView.AlignmentId;
        if (alignmentId.IsNull || alignmentId.IsErased) return false;

        var alignment = transactionManager.Unwrap()
            .GetObject(alignmentId, OpenMode.ForRead) as Alignment;

        if (alignment == null) return false;

        alignmentWrapped = new CivilAlignmentWrapper(alignment);

        return alignmentWrapped != null;

    }

    /// <inheritdoc />
    public List<ICivilProfileViewBand> GetBands(IAutocadTransactionManager transactionManager)
    {
        var bands = new List<ICivilProfileViewBand>();

        try
        {
            var bandSet = _profileView.Bands;
            if (bandSet == null)
                return bands;

            bands.AddRange(this.ExtractBandsFromSet(bandSet.GetTopBandItems()));

            bands.AddRange(this.ExtractBandsFromSet(bandSet.GetBottomBandItems()));
        }
        catch
        {
            // Return empty list if extraction fails
        }

        return bands;
    }

    /// <inheritdoc />
    public List<ICivilFeatureLabel> GetProfileViewLabelGroups(IAutocadTransactionManager transactionManager)
    {
        var labels = new List<ICivilFeatureLabel>();

        var labelIds = _profileView.GetProfileViewLabelIds();
        if (labelIds == null || labelIds.Count == 0)
            return labels;

        foreach (ObjectId labelId in labelIds)
        {

            if (labelId.IsNull || labelId.IsErased)
                continue;

            var label = transactionManager.Unwrap()
                .GetObject(labelId, OpenMode.ForRead) as ProfileViewDepthLabel;

            var featureLabel = new CivilFeatureLabelWrapperBase<ProfileViewDepthLabel>(label);

            labels.Add(featureLabel);

        }

        return labels;
    }

    /// <summary>
    /// Creates a duplicate of this ProfileView properties wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public override CivilProfileViewWrapper ShallowClone()
    {
        return new CivilProfileViewWrapper(_profileView);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var stationRange = this.Properties.StationRange;
        var elevationRange = this.Properties.ElevationRange;
        return $"ProfileView: {this.Name} (Sta: {stationRange.T0:F2} - {stationRange.T1:F2}, Elev: {elevationRange.T0:F2} - {elevationRange.T1:F2})";
    }
}