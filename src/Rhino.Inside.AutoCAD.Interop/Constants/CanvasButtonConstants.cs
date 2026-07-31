namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// Defines the dimensions of the clickable buttons rendered at the bottom of a
/// Grasshopper component capsule.
/// </summary>
/// <remarks>
/// Consumed by the canvas button implementation alone. Component attributes reserve
/// space for a button by growing their bounds by the button's reported height rather
/// than by reading these values directly.
/// </remarks>
public class CanvasButtonConstants
{
    /// <summary>
    /// The height of the button capsule, in canvas units.
    /// </summary>
    public const int ButtonHeight = 22;

    /// <summary>
    /// The gap between the button capsule and the edges of the component capsule,
    /// in canvas units. Applied on all four sides.
    /// </summary>
    public const int ButtonPadding = 3;
}
