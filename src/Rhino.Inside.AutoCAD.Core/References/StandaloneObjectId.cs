using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Core.References;

/// <summary>
/// Pure managed implementation of <see cref="IObjectId"/> for standalone out-of-process execution.
/// </summary>
public class StandaloneObjectId : IObjectId
{
    /// <summary>
    /// Default null object ID.
    /// </summary>
    public static readonly IObjectId Default = new StandaloneObjectId(0);

    /// <inheritdoc/>
    public long Value { get; }

    /// <inheritdoc/>
    public bool IsValid => Value != 0;

    /// <inheritdoc/>
    public bool IsErased => false;

    /// <summary>
    /// Initializes a new instance of <see cref="StandaloneObjectId"/>.
    /// </summary>
    public StandaloneObjectId(long value)
    {
        Value = value;
    }

    /// <inheritdoc/>
    public IObjectId ShallowClone() => new StandaloneObjectId(Value);

    /// <inheritdoc/>
    public bool IsEqualTo(IObjectId other) => other != null && Value == other.Value;

    /// <inheritdoc/>
    public override string ToString() => $"({Value})";
}
