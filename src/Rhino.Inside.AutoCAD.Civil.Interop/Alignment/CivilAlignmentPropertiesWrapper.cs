using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps properties extracted from a Civil 3D Alignment.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted alignment property information.
/// The data is captured at construction time from an <see cref="Alignment"/>.
/// </remarks>
public record CivilAlignmentProperties : ICivilAlignmentProperties
{
    private readonly Alignment _alignment;
    private readonly Transaction? _transaction;

    /// <inheritdoc />
    public string Name { get; } = string.Empty;

    /// <inheritdoc />
    public string Description { get; } = string.Empty;

    /// <inheritdoc />
    public double StartStation { get; }

    /// <inheritdoc />
    public double EndStation { get; }

    /// <inheritdoc />
    public double Length { get; }

    /// <inheritdoc />
    public Core.AlignmentType AlignmentType { get; }

    /// <inheritdoc />
    public int EntityCount { get; }

    /// <inheritdoc />
    [Obsolete("Use Site property instead.")]
    public string SiteName { get; } = string.Empty;

    /// <inheritdoc />
    public INamedId Site { get; }

    /// <inheritdoc />
    public INamedId Style { get; }

    /// <inheritdoc />
    public INamedId DesignCheckSet { get; }

    /// <inheritdoc />
    public ICivilReferenceStation ReferenceStation { get; }

    /// <inheritdoc />
    public ICivilDesignSpeeds DesignSpeeds { get; }

    /// <inheritdoc />
    public ICivilCANTInfo CANTInfo { get; }

    /// <inheritdoc />
    public ICivilConnectedAlignmentInfo ConnectedAlignmentInfo { get; }

    /// <inheritdoc />
    public ICivilOffsetAlignmentInfo OffsetAlignmentInfo { get; }

    /// <inheritdoc />
    public ICivilRailAlignmentInfo RailAlignmentInfo { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilAlignmentProperties"/>.
    /// </summary>
    /// <param name="alignment">The Civil 3D alignment to extract properties from.</param>
    public CivilAlignmentProperties(Alignment alignment)
        : this(alignment, alignment.Database.TransactionManager.TopTransaction)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilAlignmentProperties"/> with a transaction.
    /// </summary>
    /// <param name="alignment">The Civil 3D alignment to extract properties from.</param>
    /// <param name="transaction">The transaction to use for database lookups.</param>
    public CivilAlignmentProperties(Alignment alignment, Transaction? transaction)
    {
        _alignment = alignment;
        _transaction = transaction;

        // Basic properties
        this.Name = alignment.Name;
        this.Description = alignment.Description ?? string.Empty;
        this.StartStation = alignment.StartingStation;
        this.EndStation = alignment.EndingStation;
        this.Length = alignment.Length;
        this.AlignmentType = alignment.AlignmentType.ToRhinoInsideAlignmentType();
        this.EntityCount = alignment.Entities.Count;
#pragma warning disable CS0618 // Type or member is obsolete
        this.SiteName = alignment.SiteName ?? string.Empty;
#pragma warning restore CS0618

        // Site as NamedId
        Site = CreateNamedIdFromObjectId(alignment.SiteId, alignment.SiteName ?? string.Empty, transaction);

        // Style as NamedId
        Style = CreateStyleNamedId(alignment, transaction);

        // DesignCheckSet as NamedId
        DesignCheckSet = CreateDesignCheckSetNamedId(alignment, transaction);

        // Extended property types
        ReferenceStation = new CivilReferenceStation(alignment);
        DesignSpeeds = new CivilDesignSpeeds(alignment);
        CANTInfo = new CivilCANTInfo(alignment);

        // Properties that need transaction
        if (transaction != null)
        {
            ConnectedAlignmentInfo = new CivilConnectedAlignmentInfo(alignment, transaction);
            OffsetAlignmentInfo = new CivilOffsetAlignmentInfo(alignment, transaction);
        }
        else
        {
            ConnectedAlignmentInfo = CivilConnectedAlignmentInfo.Empty;
            OffsetAlignmentInfo = CivilOffsetAlignmentInfo.Empty;
        }

        RailAlignmentInfo = new CivilRailAlignmentInfo(alignment);
    }

    private static INamedId CreateNamedIdFromObjectId(ObjectId objectId, string name, Transaction? transaction)
    {
        if (objectId.IsNull)
            return NamedId.Empty;

        return new NamedId(name, objectId);
    }

    private static INamedId CreateStyleNamedId(Alignment alignment, Transaction? transaction)
    {
        try
        {
            var styleId = alignment.StyleId;
            if (styleId.IsNull)
                return NamedId.Empty;

            if (transaction != null)
            {
                var style = transaction.GetObject(styleId, OpenMode.ForRead) as AlignmentStyle;
                var styleName = style?.Name ?? string.Empty;
                return new NamedId(styleName, styleId);
            }

            return new NamedId(string.Empty, styleId);
        }
        catch
        {
            return NamedId.Empty;
        }
    }

    private static INamedId CreateDesignCheckSetNamedId(Alignment alignment, Transaction? transaction)
    {
        try
        {
            var checkSetId = alignment.DesignCheckSetId;
            if (checkSetId.IsNull)
                return NamedId.Empty;

            if (transaction != null)
            {
                var checkSet = transaction.GetObject(checkSetId, OpenMode.ForRead) as DesignCheckSet;
                var checkSetName = checkSet?.Name ?? string.Empty;
                return new NamedId(checkSetName, checkSetId);
            }

            return new NamedId(string.Empty, checkSetId);
        }
        catch
        {
            return NamedId.Empty;
        }
    }

    /// <inheritdoc />
    public ICivilAlignmentProperties Update(IAutocadTransactionManager transactionManager,
        string newName, string newDescription)
    {
        var alignment = transactionManager.Unwrap().GetObject(_alignment.Id, OpenMode.ForWrite) as Alignment;

        if (alignment == null)
        {
            return this;
        }

        alignment.Name = newName;
        alignment.Description = newDescription;

        return new CivilAlignmentProperties(alignment, transactionManager.Unwrap());
    }

    /// <summary>
    /// Creates a duplicate of this alignment properties wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilAlignmentProperties ShallowClone()
    {
        return new CivilAlignmentProperties(_alignment, _transaction);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Alignment Properties: {this.Name} (Sta: {this.StartStation:F2} - {this.EndStation:F2}, Length: {this.Length:F2})";
    }
}
