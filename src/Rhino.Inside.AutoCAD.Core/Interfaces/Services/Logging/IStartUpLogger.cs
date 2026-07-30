namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Logger for storing error messages during application start up. It is used to collect errors
/// This logger is used to collect error messages during application start up before the main
/// logging system is available. It allows for adding error messages and retrieving the most
/// recent error message, as well as checking if any errors have been logged.
/// </summary>
public interface IStartUpLogger
{
    /// <summary>
    /// Boolean property indicating whether this <see cref="IStartUpLogger"/> contains any
    /// error messages.
    /// </summary>
    bool HasError { get; }

    /// <summary>
    /// Adds the specified error message to this <see cref="IStartUpLogger"/>.
    /// </summary>
    /// <param name="message"></param>
    void AddError(string message);

    /// <summary>
    /// Gets the last error message added to this <see cref="IStartUpLogger"/>. If no error
    /// messages have been added, it returns an empty string.
    /// </summary>
    /// <returns></returns>
    string GetLastErrorMessage();

    /// <summary>
    /// Writes any messages collected before the main logging system was available into it.
    /// </summary>
    /// <remarks>
    /// Messages added once the main logger is running are written through immediately, so
    /// this only has work to do for those recorded earlier. It is safe to call at any time
    /// and does nothing if the main logger is still unavailable. Call it as soon as the main
    /// logger has been initialised, otherwise the earliest start-up failures — the ones most
    /// likely to explain why the application did not load — are never recorded to disk.
    /// <para>
    /// Flushing does not consume anything: <see cref="HasError"/> and
    /// <see cref="GetLastErrorMessage"/> continue to report every message added, so a
    /// message that reaches the log is still shown to the user unchanged.
    /// </para>
    /// </remarks>
    void Flush();
}