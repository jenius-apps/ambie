﻿using AmbientSounds.Views;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

#nullable enable

namespace AmbientSounds.Services.Uwp;

public sealed class CompactNavigator : ICompactNavigator
{
    public object? ContentFrame { get; set; }

    public void NavigateTo(CompactViewMode mode)
    {
        if (ContentFrame is Frame f)
        {
            Type pageType = mode switch
            {
                CompactViewMode.Focus => typeof(CompactFocusPage),
                CompactViewMode.Home => typeof(CompactHomePage),
                _ => typeof(CompactHomePage)
            };

            if (f.CurrentSourcePageType != pageType)
            {
                f.Navigate(pageType, null, new SuppressNavigationTransitionInfo());
            }
        }
    }
}
