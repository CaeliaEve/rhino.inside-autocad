namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Base record for Goo results that provides common success/failure tracking.
/// </summary>
/// <remarks>
/// Derived records should create a static Failed property that sets IsSuccess to false.
/// By default, IsSuccess is true for all non-failed instances.
/// </remarks>
public abstract record GooResultBase
{
    /// <summary>
    /// Gets a value indicating whether the result is successful.
    /// </summary>
    /// <remarks>
    /// Defaults to true. Set to false via object initializer for failed instances.
    /// </remarks>
    public bool IsSuccess { get; init; } = true;
}
