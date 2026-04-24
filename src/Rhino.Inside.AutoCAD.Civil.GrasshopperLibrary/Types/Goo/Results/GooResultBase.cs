namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Base class for Goo results that provides common success/failure tracking.
/// </summary>
/// <remarks>
/// Derived classes should create a static Failed property that sets IsSuccess to false.
/// By default, IsSuccess is true for all non-failed instances.
/// </remarks>
public abstract class GooResultBase
{
    /// <summary>
    /// Gets a value indicating whether the result is successful.
    /// </summary>
    /// <remarks>
    /// Defaults to true. Set to false via object initializer for failed instances.
    /// </remarks>
    public bool IsSuccess { get; protected set; } = true;
}
