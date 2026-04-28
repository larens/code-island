using System.Text.Json.Serialization;

namespace CodeIsland.Windows.Models;

/// <summary>
/// Application theme.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AppTheme
{
    [JsonPropertyName("light")]
    Light,
    [JsonPropertyName("dark")]
    Dark,
    [JsonPropertyName("system")]
    System
}

/// <summary>
/// Notification sound options.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationSound
{
    [JsonPropertyName("none")]
    None,
    [JsonPropertyName("default")]
    Default
}

/// <summary>
/// Application settings.
/// Corresponds to AppSettings in Settings.swift.
/// </summary>
public sealed class AppSettings
{
    // General
    [JsonPropertyName("launchAtStartup")]
    public bool LaunchAtStartup { get; set; }

    [JsonPropertyName("language")]
    public string Language { get; set; } = "system";

    // Display
    [JsonPropertyName("theme")]
    public AppTheme Theme { get; set; } = AppTheme.System;

    [JsonPropertyName("windowPositionRemember")]
    public bool WindowPositionRemember { get; set; } = true;

    [JsonPropertyName("windowLeft")]
    public double WindowLeft { get; set; } = double.NaN;

    [JsonPropertyName("windowTop")]
    public double WindowTop { get; set; } = double.NaN;

    [JsonPropertyName("windowWidth")]
    public double WindowWidth { get; set; } = 480;

    [JsonPropertyName("windowHeight")]
    public double WindowHeight { get; set; } = 720;

    // Hooks (which tools are enabled)
    [JsonPropertyName("enabledHooks")]
    public Dictionary<string, bool> EnabledHooks { get; set; } = [];

    // Sound
    [JsonPropertyName("notificationSound")]
    public NotificationSound NotificationSound { get; set; } = NotificationSound.Default;

    [JsonPropertyName("soundVolume")]
    public double SoundVolume { get; set; } = 0.8;

    /// <summary>
    /// Get default settings.
    /// </summary>
    public static AppSettings Defaults => new()
    {
        LaunchAtStartup = false,
        Language = "system",
        Theme = AppTheme.System,
        WindowPositionRemember = true,
        EnabledHooks = new Dictionary<string, bool>
        {
            ["claude-code"] = true,
            ["codex"] = true,
            ["gemini-cli"] = true
        },
        NotificationSound = NotificationSound.Default,
        SoundVolume = 0.8
    };
}
