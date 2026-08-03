using Rhino.Inside.AutoCAD.Core.Interfaces;
using System.Text.Json;

namespace Rhino.Inside.AutoCAD.Services;

/// <inheritdoc cref="IUserSettingsStore"/>
/// <remarks>
/// Stores the settings under AppData rather than in the installation bundle, which every
/// build and install overwrites. Reading happens on the startup path before the
/// bootstrapper exists, so this type resolves its own path and takes no dependencies.
/// </remarks>
public class UserSettingsStore : IUserSettingsStore
{
    private const string _applicationFolderName = ApplicationConstants.ApplicationFolderName;
    private const string _userSettingsJsonName = ApplicationConstants.UserSettingsJsonName;

    private static readonly Lazy<UserSettingsStore> _instance = new(() => new UserSettingsStore());

    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;

    private UserSettings _settings;

    /// <summary>
    /// The <see cref="UserSettingsStore"/> singleton instance.
    /// </summary>
    /// <remarks>
    /// Shared so that a change made on the settings page is seen by every other reader
    /// without a reload.
    /// </remarks>
    public static IUserSettingsStore Instance => _instance.Value;

    /// <inheritdoc/>
    public IUserSettings Settings => _settings;

    /// <summary>
    /// Constructs a new <see cref="UserSettingsStore"/> reading from the default location,
    /// <c>%APPDATA%\Bimorph\RhinoInsideAutoCAD\UserSettings.json</c>.
    /// </summary>
    public UserSettingsStore() : this(GetDefaultFilePath())
    { }

    /// <summary>
    /// Constructs a new <see cref="UserSettingsStore"/> reading from the given file.
    /// </summary>
    /// <param name="filePath">The full path of the user settings JSON file.</param>
    public UserSettingsStore(string filePath)
    {
        _filePath = filePath;

        _settings = Read(filePath);
    }

    /// <summary>
    /// Returns the default user settings file path.
    /// </summary>
    private static string GetDefaultFilePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        return Path.Combine(appData, _applicationFolderName, _userSettingsJsonName);
    }

    /// <summary>
    /// Reads the settings from disk, returning defaults when the file is absent or cannot
    /// be parsed. Never throws: a corrupt settings file must not prevent AutoCAD starting.
    /// </summary>
    private static UserSettings Read(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return new UserSettings();

            using var stream = File.OpenRead(filePath);

            return JsonSerializer.Deserialize<UserSettings>(stream) ?? new UserSettings();
        }
        catch (Exception e)
        {
            LoggerService.Instance.LogError(e);

            return new UserSettings();
        }
    }

    /// <inheritdoc/>
    public void Save()
    {
        try
        {
            var directory = Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(_settings, _serializerOptions);

            File.WriteAllText(_filePath, json);
        }
        catch (Exception e)
        {
            LoggerService.Instance.LogError(e);
        }
    }

    /// <inheritdoc/>
    public void Reload()
    {
        _settings = Read(_filePath);
    }
}
