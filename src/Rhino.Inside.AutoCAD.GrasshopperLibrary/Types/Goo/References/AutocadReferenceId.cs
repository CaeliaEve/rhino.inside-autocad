using Autodesk.AutoCAD.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <inheritdoc cref="IAutocadReferenceId"/>
public class AutocadReferenceId : IAutocadReferenceId
{
    /// <summary>
    /// Static constructor for when there is no reference.
    /// </summary>
    public static IAutocadReferenceId NoReference => Rhino.Inside.AutoCAD.Core.References.StandaloneReferenceId.NoReference;

    /// <summary>
    /// The Handle string which persists between AutoCAD sessions to identify the referenced object.
    /// </summary>
    private readonly string _objectHandleStr;

    /// <inheritdoc  />
    public IObjectId ObjectId { get; }

    /// <inheritdoc  />
    public bool IsValid => this.ObjectId.IsValid;

    /// <summary>
    /// Default constructor for when there is no referenced object.
    /// </summary>
    public AutocadReferenceId()
    {
        this.ObjectId = Rhino.Inside.AutoCAD.Core.References.StandaloneObjectId.Default;
        _objectHandleStr = string.Empty;
    }

    /// <summary>
    /// Constructor which references an AutoCAD Object.
    /// </summary>
    public AutocadReferenceId(IDbObject objectToReference)
    {
        this.ObjectId = objectToReference.Id;
        _objectHandleStr = objectToReference.UnwrapObject().Handle.ToString();
    }

    /// <summary>
    /// Constructor which references an AutoCAD Object by handle string.
    /// </summary>
    public AutocadReferenceId(string handleStr)
    {
        _objectHandleStr = handleStr ?? string.Empty;
        if (!string.IsNullOrEmpty(_objectHandleStr) && long.TryParse(_objectHandleStr, System.Globalization.NumberStyles.HexNumber, null, out var val))
        {
            this.ObjectId = new Rhino.Inside.AutoCAD.Core.References.StandaloneObjectId(val);
        }
        else
        {
            this.ObjectId = Rhino.Inside.AutoCAD.Core.References.StandaloneObjectId.Default;
        }
    }

    /// <summary>
    /// Constructor which references an AutoCAD Object.
    /// </summary>
    public AutocadReferenceId(DBObject objectToReference)
    {
        this.ObjectId = new AutocadObjectIdWrapper(objectToReference.Id);
        _objectHandleStr = objectToReference.Handle.ToString();
    }

    /// <inheritdoc  />
    public string GetSerializedValue()
    {
        return _objectHandleStr;
    }

    /// <inheritdoc  />
    public override string ToString()
    {
        return this.IsValid ? _objectHandleStr : "No Database Id";
    }
}