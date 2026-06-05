namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// An extension to manage the Rhino core inside AutoCAD.
/// </summary>
public interface IRhinoCoreExtension
{
    /// <summary>
    /// The <see cref="IStartUpLogger"/> a logger used for validation.
    /// </summary>
    IStartUpLogger StartUpLogger { get; }

    /// <summary>
    /// Access to the Rhino window manager.
    /// </summary>
    IRhinoWindowManager WindowManager { get; }

    /// <summary>
    /// Ensures that the Rhino core is, created and running, if there is not an existing
    /// instance then it creates one.
    /// </summary>
    void ValidateRhinoCore();

    /// <summary>
    /// The steps to take to shut down this rhino inside extension.
    /// </summary>
    void Shutdown();

    /// <summary>
    /// We need to check if Rhino is in get mode for grasshopper and show the window if it is headless.
    /// </summary>
    void ShowInGet();
}