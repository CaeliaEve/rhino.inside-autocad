using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Custom attributes for <see cref="GetAutocadObjectsByFilterComponent"/> that displays
/// a "Query" button when Auto Update is disabled.
/// </summary>
public class GetAutocadObjectsByFilterComponentAttributes : CanvasButtonComponentAttributes
{
    private const string QueryButtonText = GrasshopperMessages.QueryButton;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAutocadObjectsByFilterComponentAttributes"/> class.
    /// </summary>
    public GetAutocadObjectsByFilterComponentAttributes(GetAutocadObjectsByFilterComponent owner)
        : base(owner, CreateQueryButton(owner))
    {
    }

    /// <summary>
    /// Creates the Query button for the specified owner. A static factory because the
    /// button must be supplied to the base constructor.
    /// </summary>
    private static ICanvasButton CreateQueryButton(GetAutocadObjectsByFilterComponent owner)
    {
        var queryButton = new CanvasButton(QueryButtonText, owner.TriggerManualUpdate);

        return queryButton;
    }

    private GetAutocadObjectsByFilterComponent TypedOwner
        => (GetAutocadObjectsByFilterComponent)this.Owner;

    /// <inheritdoc />
    protected override bool IsButtonVisible
        => this.TypedOwner.AutoUpdateEnabled == false;
}
