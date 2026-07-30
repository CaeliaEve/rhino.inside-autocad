namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Contains exceptions raised inside handlers that AutoCAD invokes across the
/// managed/native boundary.
/// </summary>
/// <remarks>
/// AutoCAD raises document and database events from native reactors, reaching managed
/// handlers through a reverse P/Invoke stub. No frame above such a handler is managed, so
/// nothing there can catch an escaping exception and .NET terminates the process. Every
/// handler subscribed to an AutoCAD event must therefore contain its own failures, as must
/// any <c>async void</c> method, whose exceptions are rethrown on the dispatcher with no
/// caller to receive them.
/// </remarks>
public interface IAutocadGuard
{
    /// <summary>
    /// Invokes <paramref name="handler"/>, containing and logging any exception it raises.
    /// </summary>
    /// <param name="handler">
    /// The handler body to invoke.
    /// </param>
    /// <param name="context">
    /// The name of the handler, recorded against any failure.
    /// </param>
    void Run(Action handler, string context);
}
