using System.Text.Json;
using CodeIsland.Windows.Models;

namespace CodeIsland.Windows.Services;

/// <summary>
/// Settings persistence service.
/// Reads/writes settings to %LOCALAPPDATA%\CodeIsland\settings.json.
/// </summary>
public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly string SettingsDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CodeIsland");

    private static readonly string SettingsFilePath = Path.Combine(
        SettingsDirectory,
        "settings.json");

    private AppSettings _settings = AppSettings.Defaults;

    public AppSettings Settings => _settings;

    public event EventHandler<AppSettings>? SettingsChanged;

    public void Load()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                var json = File.ReadAllText(SettingsFilePath);
                _settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? AppSettings.Defaults;
            }
        }
        catch
        {
            _settings = AppSettings.Defaults;
        }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(SettingsDirectory);
            var json = JsonSerializer.Serialize(_settings, JsonOptions);
            File.WriteAllText(SettingsFilePath, json);
            SettingsChanged?.Invoke(this, _settings);
        }
        catch
        {
            // Best-effort save
        }
    }

    public void Update(Action<AppSettings> update)
    {
        update(_settings);
        Save();
    }
}
