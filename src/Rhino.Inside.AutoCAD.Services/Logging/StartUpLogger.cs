using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Services;

/// <inheritdoc cref="IStartUpLogger"/>
public class StartUpLogger : IStartUpLogger
{
    /// <summary>
    /// Every message added, retained for the lifetime of this logger.
    /// </summary>
    /// <remarks>
    /// Never drained. <see cref="Flush"/> only advances <see cref="_pendingMessages"/>, so
    /// what <see cref="GetLastErrorMessage"/> reports to the user is unaffected by whether
    /// the message has also reached the log file. A flushed message is therefore recorded
    /// twice on purpose: once in the log, and once here for the start-up dialog.
    /// </remarks>
    private readonly List<string> _errorMessages = new();

    /// <summary>
    /// Messages not yet written to <see cref="LoggerService"/>, held only to track what the
    /// next <see cref="Flush"/> still owes the log.
    /// </summary>
    private readonly Queue<string> _pendingMessages = new();

    /// <inheritdoc />
    public bool HasError => _errorMessages.Count > 0;

    /// <inheritdoc />
    public void AddError(string message)
    {
        _errorMessages.Add(message);

        _pendingMessages.Enqueue(message);

        this.Flush();
    }

    /// <inheritdoc />
    public string GetLastErrorMessage()
    {
        return this.HasError ? _errorMessages.Last() : string.Empty;
    }

    /// <inheritdoc />
    public void Flush()
    {
        if (LoggerService.IsInitialized == false) return;

        var logger = LoggerService.Instance;

        while (_pendingMessages.Count > 0)
        {
            var message = _pendingMessages.Dequeue();

            logger.LogError(message);
        }
    }
}
