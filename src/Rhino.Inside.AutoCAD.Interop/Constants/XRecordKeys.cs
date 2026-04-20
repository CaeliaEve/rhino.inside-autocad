using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// Defines DXF group code constants used for storing and retrieving extended data (XData)
/// in AutoCAD XRecord entries.
/// </summary>
/// <remarks>
/// These constants correspond to standard AutoCAD DXF group codes for extended entity data.
/// XRecords provide a mechanism for persisting custom application data within the drawing database.
/// </remarks>
/// <seealso cref="IXRecord"/>
public class XRecordKeys
{
    /// <summary>
    /// DXF group code for the registered application name (e.g., "Rhino.Inside.AutoCAD").
    /// </summary>
    /// <remarks>
    /// This key identifies extended data ownership. The application name must be registered
    /// in the drawing's APPID table before use. Changing this value will break compatibility
    /// with existing drawings containing persisted data.
    /// </remarks>
    public const short ApplicationNameKey = 1001;

    /// <summary>
    /// DXF group code for storing the <see cref="IAutocadDocument.Id"/> as extended data.
    /// </summary>
    /// <remarks>
    /// Used to associate XRecord entries with their parent document.
    /// This enables document-specific data retrieval across sessions.
    /// </remarks>
    public const short DocumentIdKey = 1000;
}