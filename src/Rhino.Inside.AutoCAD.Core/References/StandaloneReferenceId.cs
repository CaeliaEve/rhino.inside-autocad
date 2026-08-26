using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Core.References;

/// <summary>
/// Pure managed implementation of <see cref="IAutocadReferenceId"/> for standalone out-of-process execution.
/// </summary>
public class StandaloneReferenceId : IAutocadReferenceId
{
    /// <summary>
    /// Default instance when no reference is associated.
    /// </summary>
    public static readonly IAutocadReferenceId NoReference = new StandaloneReferenceId(string.Empty);

    private readonly string _handleString;
    private readonly long _handleValue;

    /// <inheritdoc/>
    public IObjectId ObjectId { get; }

    /// <inheritdoc/>
    public bool IsValid => !string.IsNullOrEmpty(_handleString) && _handleValue != 0;

    /// <summary>
    /// Initializes a new instance of <see cref="StandaloneReferenceId"/> from a handle string.
    /// </summary>
    public StandaloneReferenceId(string handleStr)
    {
        _handleString = handleStr ?? string.Empty;
        if (!string.IsNullOrEmpty(_handleString) && long.TryParse(_handleString, System.Globalization.NumberStyles.HexNumber, null, out var val))
        {
            _handleValue = val;
            ObjectId = new StandaloneObjectId(val);
        }
        else
        {
            _handleValue = 0;
            ObjectId = StandaloneObjectId.Default;
        }
    }

    /// <inheritdoc/>
    public string GetSerializedValue() => _handleString;

    /// <inheritdoc/>
    public override string ToString() => IsValid ? _handleString : "No Reference Handle";
}
