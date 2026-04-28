using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CodeIsland.Windows.Helpers;
using CodeIsland.Windows.Models;
using CodeIsland.Windows.Services;

namespace CodeIsland.Windows.ViewModels;

/// <summary>
/// ViewModel for the settings page.
/// </summary>
public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;
    private readonly HookInstaller _hookInstaller;

    // General
    [ObservableProperty]
    private bool _launchAtStartup;

    [ObservableProperty]
    private string _selectedLanguage = "system";

    // Display
    [ObservableProperty]
    private string _selectedTheme = "system";

    // Sound
    [ObservableProperty]
    private string _selectedSound = "default";

    [ObservableProperty]
    private double _soundVolume = 0.8;

    // Hooks
    [ObservableProperty]
    private ObservableCollection<ClientProfileStatus> _hookProfiles = [];

    public List<string> Languages { get; } = ["system", "English", "简体中文"];
    public List<string> Themes { get; } = ["system", "light", "dark"];
    public List<string> Sounds { get; } = ["none", "default"];

    public SettingsViewModel(SettingsService settingsService, HookInstaller hookInstaller)
    {
        _settingsService = settingsService;
        _hookInstaller = hookInstaller;
        LoadSettings();
    }

    private void LoadSettings()
    {
        var settings = _settingsService.Settings;
        LaunchAtStartup = settings.LaunchAtStartup;
        SelectedLanguage = settings.Language;
        SelectedTheme = settings.Theme.ToString().ToLowerInvariant();
        SelectedSound = settings.NotificationSound.ToString().ToLowerInvariant();
        SoundVolume = settings.SoundVolume;
        HookProfiles = new ObservableCollection<ClientProfileStatus>(_hookInstaller.GetProfileStatuses());
    }

    [RelayCommand]
    private void SaveSettings()
    {
        _settingsService.Update(s =>
        {
            s.LaunchAtStartup = LaunchAtStartup;
            s.Language = SelectedLanguage;
            s.Theme = Enum.TryParse<AppTheme>(SelectedTheme, true, out var theme) ? theme : AppTheme.System;
            s.NotificationSound = Enum.TryParse<NotificationSound>(SelectedSound, true, out var sound) ? sound : NotificationSound.Default;
            s.SoundVolume = SoundVolume;

            // Save hook enabled states
            foreach (var profile in HookProfiles)
            {
                s.EnabledHooks[profile.Profile.Id] = profile.Enabled;
            }
        });

        StartupHelper.SetStartupEnabled(LaunchAtStartup);
    }

    [RelayCommand]
    private void InstallHook(ClientProfileStatus profile)
    {
        if (_hookInstaller.InstallHook(profile.Profile))
        {
            // Refresh profiles
            HookProfiles = new ObservableCollection<ClientProfileStatus>(_hookInstaller.GetProfileStatuses());
        }
    }

    [RelayCommand]
    private void UninstallHook(ClientProfileStatus profile)
    {
        if (_hookInstaller.UninstallHook(profile.Profile))
        {
            HookProfiles = new ObservableCollection<ClientProfileStatus>(_hookInstaller.GetProfileStatuses());
        }
    }
}
