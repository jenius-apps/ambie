using AmbientSounds.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                CompactViewMode.Home => typeof(CompactHomePage),
                _ => typeof(FocusPage)
            };

            f.Navigate(pageType, null, new SuppressNavigationTransitionInfo());
        }
    }
}
