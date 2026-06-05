namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Defines a filter for selecting AutoCAD entities based on specific criteria.
/// </summary>
/// <remarks>
/// Filters encapsulate AutoCAD selection set filter logic using DXF group codes and values.
/// Common implementations include type filters (lines, circles), layer filters, and
/// compound filters (AND/OR combinations). Used by Grasshopper components such as
/// GetAutocadObjectsByFilterComponent, ObjectByLayerFilterComponent, and the various
/// converter components to query entities from a document.
/// </remarks>
/// <seealso cref="IAutocadSelectionFilterWrapper"/>
/// <seealso cref="ITypedValueWrapper"/>
public interface IObjectFilter
{
    /// <summary>
    /// Creates the <see cref="IAutocadSelectionFilterWrapper"/> representing this filter's criteria.
    /// </summary>
    /// <returns>
    /// An <see cref="IAutocadSelectionFilterWrapper"/> containing the DXF filter criteria
    /// that can be passed to AutoCAD's selection methods.
    /// </returns>
    /// <remarks>
    /// The returned wrapper contains typed values with DXF group codes. For example,
    /// entity type filters use group code 0 with values like "LINE" or "CIRCLE".
    /// </remarks>
    IAutocadSelectionFilterWrapper GetSelectionFilter();

    /// <summary>
    /// Determines if a document change could affect the results of this filter.
    /// </summary>
    /// <param name="change">The document change to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the change could affect objects matching this filter's criteria;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method is used by components to determine if they need to be expired
    /// (re-evaluated) in response to document changes. Implementations should return
    /// <c>true</c> if any changed object matches the filter's entity type criteria.
    /// </remarks>
    bool IsAffectedByChange(IAutocadDocumentChange change);
}