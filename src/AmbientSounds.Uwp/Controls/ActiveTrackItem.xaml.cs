using AmbientSounds.Constants;
using JeniusApps.Common.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.UI.Helpers;
using System;
using System.Windows.Input;
using Windows.UI;
using Windows.UI.Xaml;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class ActiveTrackItem : ObservableUserControl
{
    private readonly ThemeListener _themeListener = new();
    private readonly IUserSettings _userSettings;

    public static readonly DependencyProperty CloseCommandParameterProperty = DependencyProperty.Register(
        nameof(CloseCommandParameter),
        typeof(object),
        typeof(ActiveTrackItem),
        new PropertyMetadata(null));

    public static readonly DependencyProperty CloseCommandProperty = DependencyProperty.Register(
        nameof(CloseCommand),
        typeof(ICommand),
        typeof(ActiveTrackItem),
        new PropertyMetadata(null));

    public static readonly DependencyProperty ColourHexProperty = DependencyProperty.Register(
        nameof(ColourHex),
        typeof(string),
        typeof(ActiveTrackItem),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty VolumeProperty = DependencyProperty.Register(
        nameof(Volume),
        typeof(double),
        typeof(ActiveTrackItem),
        new PropertyMetadata(1d));

    public static readonly DependencyProperty SoundNameProperty = DependencyProperty.Register(
        nameof(SoundName),
        typeof(string),
        typeof(ActiveTrackItem),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty CurrentThemeProperty = DependencyProperty.Register(
        nameof(CurrentTheme),
        typeof(string),
        typeof(ActiveTrackItem),
        new PropertyMetadata(string.Empty));

    public ActiveTrackItem()
    {
        this.InitializeComponent();
        _userSettings = App.Services.GetRequiredService<IUserSettings>();
        _userSettings.SettingSet += OnSettingSet;
        _themeListener.ThemeChanged += OnSystemThemeChanged;
        this.Unloaded += (s, e) => 
        { 
            _userSettings.SettingSet -= OnSettingSet; 
        };

        CurrentTheme = GetCurrentTheme();
    }

    private void OnSettingSet(object sender, string e)
    {
        if (e == UserSettingsConstants.Theme)
        {
            CurrentTheme = GetCurrentTheme();
        }
    }

    private void OnSystemThemeChanged(ThemeListener sender)
    {
        CurrentTheme = GetCurrentTheme();
    }

    private string GetCurrentTheme()
    {
        var userThemeSetting = _userSettings.Get<string>(UserSettingsConstants.Theme);
        if (userThemeSetting is "default")
        {
            return _themeListener.CurrentThemeName;
        }
        else
        {
            return userThemeSetting ?? string.Empty;
        }
    }

    private Color GetTopGradient(string theme)
    {
        if (theme.Equals("dark", StringComparison.OrdinalIgnoreCase))
        {
            return UIHelper.ToDarkerColour(ColourHex, 0.1);
        }
        else if (theme.Equals("light", StringComparison.OrdinalIgnoreCase))
        {
            return UIHelper.ToLighterColour(ColourHex, 0.3);
        }

        return Colors.Transparent;
    }

    private Color GetBottomGradient(string theme)
    {
        if (theme.Equals("dark", StringComparison.OrdinalIgnoreCase))
        {
            return UIHelper.ToDarkerColour(ColourHex, 0.05);
        }
        else if (theme.Equals("light", StringComparison.OrdinalIgnoreCase))
        {
            return UIHelper.ToLighterColour(ColourHex, 0.2);
        }

        return Colors.Transparent;
    }

    public string CurrentTheme
    {
        get => (string)GetValue(CurrentThemeProperty);
        set => SetValue(CurrentThemeProperty, value);
    }

    public string SoundName
    {
        get => (string)GetValue(SoundNameProperty);
        set => SetValue(SoundNameProperty, value);
    }

    public double Volume
    {
        get => (double)GetValue(VolumeProperty);
        set => SetValueDp(VolumeProperty, value);
    }

    public string ColourHex
    {
        get => (string)GetValue(ColourHexProperty);
        set => SetValue(ColourHexProperty, value);
    }

    public ICommand CloseCommand
    {
        get => (ICommand)GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    public object CloseCommandParameter
    {
        get => GetValue(CloseCommandParameterProperty);
        set => SetValue(CloseCommandParameterProperty, value);
    }

    private string FormatDeleteMessage(string soundName)
    {
        return string.Format(Strings.Resources.RemoveActiveButton, soundName);
    }
}
