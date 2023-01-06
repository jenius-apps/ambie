﻿using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class SettingsControl : UserControl
    {
        public SettingsControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<SettingsViewModel>();
        }

        public SettingsViewModel ViewModel => (SettingsViewModel)this.DataContext;

        private string Version => SystemInformation.Instance.ApplicationVersion.ToFormattedString();
    }
}