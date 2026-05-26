namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents an individual entity (segment) from a Civil 3D Alignment.
/// </summary>
public interface ICivilAlignmentEntity : ICivilAlignmentEntityBase
{
    /// <summary>
    /// The SubEntities collection provides access to any sub-entities that make up this alignment entity.
    /// </summary>
    List<ICivilAlignmentSubEntity> SubEntities { get; }
}