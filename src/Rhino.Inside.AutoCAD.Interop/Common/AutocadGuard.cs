using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Services;
using System.Diagnostics;
using AutocadException = Autodesk.AutoCAD.Runtime.Exception;

namespace Rhino.Inside.AutoCAD.Interop;

/// <inheritdoc cref="IAutocadGuard"/>
public class AutocadGuard : IAutocadGuard
{
    /// <inheritdoc/>
    public void Run(Action handler, string context)
    {
        try
        {
            handler.Invoke();
        }
        catch (AutocadException exception)
        {
            var message = $"{context} failed with AutoCAD ErrorStatus {exception.ErrorStatus}.";

            this.LogFailure(exception, message);
        }
        catch (Exception exception)
        {
            var message = $"{context} failed.";

            this.LogFailure(exception, message);
        }
    }

    /// <summary>
    /// Records a contained failure without ever throwing.
    /// </summary>
    /// <remarks>
    /// <see cref="LoggerService.Instance"/> throws when the logger has not been initialised
    /// and after <see cref="ILoggerService.Shutdown"/> clears it, so the null-conditional
    /// operator gives no protection here: the getter itself is what throws. Documents are
    /// still closed after the logger is shut down during application exit, so this window
    /// is reached in practice and the <see cref="Debug"/> write is the only diagnostic
    /// available within it.
    /// </remarks>
    private void LogFailure(Exception exception, string message)
    {
        Debug.WriteLine($"{message} {exception}");

        try
        {
            LoggerService.Instance.LogError(exception, message);
        }
        catch
        {
            // Deliberately empty: a guard that throws defeats its own purpose.
        }
    }
}
