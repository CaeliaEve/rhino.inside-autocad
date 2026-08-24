using System;

namespace Rhino.Inside.AutoCAD.Core.Host;

/// <summary>
/// Decoupled host context abstraction providing access to the in-process AutoCAD host application.
/// Allows Grasshopper and Interop libraries to function cleanly without compile-time dependency on Applications.dll.
/// </summary>
public static class AutoCadHostContext
{
    private static Func<object?>? _activeDocumentGetter;
    private static Func<object?>? _applicationGetter;
    private static Func<bool>? _initializationEnsurer;

    /// <summary>
    /// Registers the host application delegates (called by Applications.dll at host startup).
    /// </summary>
    public static void RegisterHost(Func<object?> applicationGetter, Func<object?> activeDocumentGetter, Func<bool>? initializationEnsurer = null)
    {
        _applicationGetter = applicationGetter;
        _activeDocumentGetter = activeDocumentGetter;
        _initializationEnsurer = initializationEnsurer;
    }

    /// <summary>
    /// Gets the current in-process AutoCAD host application instance (or null if running out-of-process).
    /// </summary>
    public static object? HostApplication => _applicationGetter?.Invoke();

    /// <summary>
    /// Gets the active AutoCAD document (or null if running out-of-process).
    /// </summary>
    public static object? ActiveDocument => _activeDocumentGetter?.Invoke();

    /// <summary>
    /// Gets a value indicating whether the current process is the in-process AutoCAD host.
    /// </summary>
    public static bool IsInProcessHost => _applicationGetter != null && HostApplication != null;

    /// <summary>
    /// Ensures that the host application is initialized if in-process.
    /// </summary>
    public static bool EnsureInitialized() => _initializationEnsurer?.Invoke() ?? false;
}
