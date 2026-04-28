using System.Text.Json;
using CodeIsland.Windows.Models;

namespace CodeIsland.Windows.Services;

/// <summary>
/// Hook installation logic for Windows.
/// Detects tool config paths and installs bridge hooks.
/// Corresponds to HookInstaller.swift.
/// </summary>
public sealed class HookInstaller
{
    private readonly SettingsService _settings;

    public HookInstaller(SettingsService settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// Get the resolved configuration path for a client profile on Windows.
    /// </summary>
    public string? GetConfigurationPath(ManagedHookClientProfile profile)
    {
        foreach (var path in profile.WindowsConfigurationPaths)
        {
            var resolved = Environment.ExpandEnvironmentVariables(path);
            if (File.Exists(resolved))
                return resolved;
        }
        return null;
    }

    /// <summary>
    /// Check if a hook is installed for the given profile.
    /// </summary>
    public bool IsHookInstalled(ManagedHookClientProfile profile)
    {
        var configPath = GetConfigurationPath(profile);
        if (configPath == null) return false;

        try
        {
            var json = File.ReadAllText(configPath);
            return json.Contains("codeisland") || json.Contains("code-island");
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Install hook for the given profile.
    /// </summary>
    public bool InstallHook(ManagedHookClientProfile profile)
    {
        var configPath = GetConfigurationPath(profile);
        if (configPath == null) return false;

        try
        {
            var configDir = Path.GetDirectoryName(configPath);
            if (configDir != null)
                Directory.CreateDirectory(configDir);

            Dictionary<string, object> config;
            if (File.Exists(configPath))
            {
                var existingJson = File.ReadAllText(configPath);
                config = JsonSerializer.Deserialize<Dictionary<string, object>>(existingJson)
                    ?? new Dictionary<string, object>();
            }
            else
            {
                config = new Dictionary<string, object>();
            }

            // Add hook configuration
            var hookConfig = new Dictionary<string, object>
            {
                ["enabled"] = true,
                ["bridge"] = new Dictionary<string, object>
                {
                    ["command"] = GetBridgeCommand(profile),
                    ["args"] = profile.BridgeExtraArguments,
                    ["socketPath"] = "\\\\.\\pipe\\codeisland"
                },
                ["events"] = profile.Events.Select(e => new Dictionary<string, object>
                {
                    ["name"] = e.Name,
                    ["templates"] = e.Templates
                }).ToList()
            };

            config["hooks"] = hookConfig;

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(configPath, json);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Uninstall hook for the given profile.
    /// </summary>
    public bool UninstallHook(ManagedHookClientProfile profile)
    {
        var configPath = GetConfigurationPath(profile);
        if (configPath == null || !File.Exists(configPath)) return false;

        try
        {
            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<Dictionary<string, object>>(json)
                ?? new Dictionary<string, object>();

            if (config.ContainsKey("hooks"))
            {
                config.Remove("hooks");
                var options = new JsonSerializerOptions { WriteIndented = true };
                var newJson = JsonSerializer.Serialize(config, options);
                File.WriteAllText(configPath, newJson);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string GetBridgeCommand(ManagedHookClientProfile profile)
    {
        // On Windows, the bridge is typically invoked via npx or a local binary
        return profile.Id switch
        {
            "claude-code" => "npx",
            "codex" => "npx",
            _ => "npx"
        };
    }

    /// <summary>
    /// Get all client profiles with their installation status.
    /// </summary>
    public List<ClientProfileStatus> GetProfileStatuses()
    {
        return ClientProfileRegistry.Profiles.Select(profile => new ClientProfileStatus
        {
            Profile = profile,
            ConfigExists = GetConfigurationPath(profile) != null,
            HookInstalled = IsHookInstalled(profile),
            Enabled = _settings.Settings.EnabledHooks.GetValueOrDefault(profile.Id, profile.DefaultEnabled)
        }).ToList();
    }
}

/// <summary>
/// Status of a client profile including installation state.
/// </summary>
public sealed class ClientProfileStatus
{
    public ManagedHookClientProfile Profile { get; init; } = new();
    public bool ConfigExists { get; init; }
    public bool HookInstalled { get; init; }
    public bool Enabled { get; init; }
}
