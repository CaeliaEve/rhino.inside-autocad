using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps offset alignment information extracted from a Civil 3D Alignment.
/// </summary>
public record CivilOffsetAlignmentInfo : ICivilOffsetAlignmentInfo
{
    /// <inheritdoc />
    public bool IsOffsetAlignment { get; }

    /// <inheritdoc />
    public INamedId ParentAlignment { get; }

    /// <inheritdoc />
    public double NominalOffset { get; }

    /// <inheritdoc />
    public string OffsetSide { get; }

    /// <summary>
    /// Gets an empty offset alignment info instance.
    /// </summary>
    public static CivilOffsetAlignmentInfo Empty { get; } = new();

    /// <summary>
    /// Initializes a new empty instance of <see cref="CivilOffsetAlignmentInfo"/>.
    /// </summary>
    private CivilOffsetAlignmentInfo()
    {
        IsOffsetAlignment = false;
        ParentAlignment = NamedId.Empty;
        NominalOffset = 0;
        OffsetSide = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilOffsetAlignmentInfo"/> from an Alignment.
    /// </summary>
    /// <param name="alignment">The Civil 3D alignment to extract offset info from.</param>
    /// <param name="transaction">The transaction to use for database lookups.</param>
    public CivilOffsetAlignmentInfo(Alignment alignment, Transaction transaction)
    {
        try
        {
            IsOffsetAlignment = alignment.IsOffsetAlignment;

            if (!IsOffsetAlignment)
            {
                ParentAlignment = NamedId.Empty;
                NominalOffset = 0;
                OffsetSide = string.Empty;
                return;
            }

            var parentId = alignment.OffsetAlignmentInfo?.ParentAlignmentId ?? ObjectId.Null;

            if (!parentId.IsNull)
            {
                var parentAlignmentObj = transaction.GetObject(parentId, OpenMode.ForRead) as Alignment;
                ParentAlignment = parentAlignmentObj != null
                    ? new NamedId(parentAlignmentObj.Name, parentId)
                    : NamedId.Empty;
            }
            else
            {
                ParentAlignment = NamedId.Empty;
            }

            var offsetInfo = alignment.OffsetAlignmentInfo;
            NominalOffset = offsetInfo?.NominalOffset ?? 0;
            OffsetSide = offsetInfo?.Side.ToString() ?? string.Empty;
        }
        catch
        {
            IsOffsetAlignment = false;
            ParentAlignment = NamedId.Empty;
            NominalOffset = 0;
            OffsetSide = string.Empty;
        }
    }

    /// <inheritdoc />
    public ICivilOffsetAlignmentInfo ShallowClone()
    {
        return this with { };
    }

    /// <summary>
    /// Returns a string representation of this offset alignment information.
    /// </summary>
    public override string ToString()
    {
        if (!IsOffsetAlignment)
            return "Not Offset Alignment";

        return $"Offset {NominalOffset:F2} {OffsetSide} from {ParentAlignment.Name}";
    }
}
