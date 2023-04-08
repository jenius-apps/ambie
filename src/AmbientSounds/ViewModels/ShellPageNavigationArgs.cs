﻿using AmbientSounds.Services;

namespace AmbientSounds.ViewModels;

public sealed class ShellPageNavigationArgs
{
    public ContentPageType? FirstPageOverride { get; init; }

    public string LaunchArguments { get; init; } = string.Empty;
}
