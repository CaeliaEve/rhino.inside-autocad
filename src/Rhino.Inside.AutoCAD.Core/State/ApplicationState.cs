namespace Rhino.Inside.AutoCAD.Core.State;

/// <summary>
/// Provides application-wide state management, particularly for shutdown coordination.
/// </summary>
public static class ApplicationState
{
    private static volatile bool _isShuttingDown;

    /// <summary>
    /// Gets whether the application is currently shutting down.
    /// </summary>
    public static bool IsShuttingDown => _isShuttingDown;

    /// <summary>
    /// Marks the application as shutting down. This should be called early in the
    /// shutdown process to prevent event handlers from accessing disposed resources.
    /// </summary>
    public static void BeginShutdown() => _isShuttingDown = true;
}
