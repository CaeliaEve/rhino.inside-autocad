using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps connected alignment information extracted from a Civil 3D Alignment.
/// </summary>
public record CivilConnectedAlignmentInfo : ICivilConnectedAlignmentInfo
{
    /// <inheritdoc />
    public INamedId ParentAlignment { get; }

    /// <inheritdoc />
    public IReadOnlyList<INamedId> ChildAlignments { get; }

    /// <inheritdoc />
    public bool IsConnected { get; }

    /// <summary>
    /// Gets an empty connected alignment info instance.
    /// </summary>
    public static CivilConnectedAlignmentInfo Empty { get; } = new();

    /// <summary>
    /// Initializes a new empty instance of <see cref="CivilConnectedAlignmentInfo"/>.
    /// </summary>
    private CivilConnectedAlignmentInfo()
    {
        ParentAlignment = NamedId.Empty;
        ChildAlignments = Array.Empty<INamedId>();
        IsConnected = false;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilConnectedAlignmentInfo"/> from an Alignment.
    /// </summary>
    /// <param name="alignment">The Civil 3D alignment to extract connection info from.</param>
    /// <param name="transaction">The transaction to use for database lookups.</param>
    public CivilConnectedAlignmentInfo(Alignment alignment, Transaction transaction)
    {
        try
        {
            // Get parent alignment if this is a connected alignment
            if (alignment.IsConnectedAlignment)
            {
                var parentId = alignment.ConnectedAlignmentParentId;
                if (!parentId.IsNull)
                {
                    var parentAlignment = transaction.GetObject(parentId, OpenMode.ForRead) as Alignment;
                    ParentAlignment = parentAlignment != null
                        ? new NamedId(parentAlignment.Name, parentId)
                        : NamedId.Empty;
                }
                else
                {
                    ParentAlignment = NamedId.Empty;
                }
            }
            else
            {
                ParentAlignment = NamedId.Empty;
            }

            // Get child alignments
            var children = new List<INamedId>();
            var childIds = alignment.GetConnectedAlignmentIds();

            if (childIds != null)
            {
                foreach (ObjectId childId in childIds)
                {
                    if (!childId.IsNull)
                    {
                        var childAlignment = transaction.GetObject(childId, OpenMode.ForRead) as Alignment;
                        if (childAlignment != null)
                        {
                            children.Add(new NamedId(childAlignment.Name, childId));
                        }
                    }
                }
            }

            ChildAlignments = children;
            IsConnected = ParentAlignment.IsValid || children.Count > 0;
        }
        catch
        {
            ParentAlignment = NamedId.Empty;
            ChildAlignments = Array.Empty<INamedId>();
            IsConnected = false;
        }
    }

    /// <inheritdoc />
    public ICivilConnectedAlignmentInfo ShallowClone()
    {
        return this with { };
    }

    /// <summary>
    /// Returns a string representation of this connected alignment information.
    /// </summary>
    public override string ToString()
    {
        if (!IsConnected)
            return "Not Connected";

        var parts = new List<string>();

        if (ParentAlignment.IsValid)
            parts.Add($"Parent: {ParentAlignment.Name}");

        if (ChildAlignments.Count > 0)
            parts.Add($"{ChildAlignments.Count} children");

        return $"Connected: {string.Join(", ", parts)}";
    }
}
