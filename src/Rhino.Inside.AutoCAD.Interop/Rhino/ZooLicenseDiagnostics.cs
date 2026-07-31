#if NET8_0_OR_GREATER
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// Temporary diagnostics for the LAN Zoo licensing failure on .NET 8 hosts.
/// ZooClient swallows the real exception behind a generic "communication
/// channel" message, so this logs assembly resolution events and candidate
/// exceptions to %TEMP%\RhinoInside.AutoCAD.ZooDiag.log.
/// Remove once the Zoo licensing issue is resolved.
/// </summary>
internal static class ZooLicenseDiagnostics
{
    private static readonly object _lock = new();

    private static readonly string _logPath =
        Path.Combine(Path.GetTempPath(), "RhinoInside.AutoCAD.ZooDiag.log");

    [ThreadStatic] private static bool _inHandler;

    public static void Install()
    {
        Log($"=== diagnostics installed, pid={Environment.ProcessId}, " +
            $"host={Path.GetFileName(Environment.ProcessPath)}, " +
            $"runtime={System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription} ===");

        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
        AppDomain.CurrentDomain.FirstChanceException += OnFirstChanceException;
    }

    private static Assembly? OnAssemblyResolve(object? sender, ResolveEventArgs args)
    {
        Log($"RESOLVE request: {args.Name} " +
            $"(requested by {args.RequestingAssembly?.GetName().Name ?? "<unknown>"})");

        return null;
    }

    private static void OnAssemblyLoad(object? sender, AssemblyLoadEventArgs args)
    {
        var assembly = args.LoadedAssembly;
        var name = assembly.GetName().Name ?? string.Empty;

        if (IsInteresting(name))
            Log($"LOADED: {assembly.FullName} from " +
                $"{(assembly.IsDynamic ? "<dynamic>" : assembly.Location)}");
    }

    private static void OnFirstChanceException(object? sender, FirstChanceExceptionEventArgs args)
    {
        if (_inHandler)
            return;

        _inHandler = true;
        try
        {
            var ex = args.Exception;
            var typeName = ex.GetType().FullName ?? string.Empty;

            var interesting =
                typeName.Contains("ServiceModel") ||
                ex is FileNotFoundException
                    or FileLoadException
                    or TypeLoadException
                    or TypeInitializationException
                    or MissingMethodException
                    or BadImageFormatException
                    or PlatformNotSupportedException ||
                (ex.Message?.IndexOf("Zoo", StringComparison.OrdinalIgnoreCase) >= 0) ||
                (ex.Message?.IndexOf("communication channel", StringComparison.OrdinalIgnoreCase) >= 0);

            if (interesting)
                Log($"EXCEPTION: {ex}");
        }
        catch
        {
            // Never let diagnostics disturb the host.
        }
        finally
        {
            _inHandler = false;
        }
    }

    private static bool IsInteresting(string assemblyName) =>
        assemblyName.Contains("ServiceModel", StringComparison.OrdinalIgnoreCase) ||
        assemblyName.Contains("Zoo", StringComparison.OrdinalIgnoreCase) ||
        assemblyName.Contains("Rhino", StringComparison.OrdinalIgnoreCase) ||
        assemblyName.Contains("crypto", StringComparison.OrdinalIgnoreCase) ||
        assemblyName.Contains("Proxy", StringComparison.OrdinalIgnoreCase);

    private static void Log(string message)
    {

        try
        {
            lock (_lock)
            {
                File.AppendAllText(
                    _logPath,
                    $"{DateTime.Now:HH:mm:ss.fff} [{Environment.CurrentManagedThreadId}] {message}{Environment.NewLine}");
            }
        }
        catch
        {
            // Never let diagnostics disturb the host.
        }
    }
}
#endif
