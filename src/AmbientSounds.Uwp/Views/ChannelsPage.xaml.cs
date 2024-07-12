using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

#nullable enable

namespace AmbientSounds.Views;

public sealed partial class ChannelsPage : Page
{
    private CancellationTokenSource? _cts;

    public ChannelsPage()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<ChannelsPageViewModel>();
    }

    public ChannelsPageViewModel ViewModel => (ChannelsPageViewModel)this.DataContext;

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _cts ??= new();

        try
        {
            await ViewModel.InitializeAsync(_cts.Token);
        }
        catch (OperationCanceledException)
        {

        }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        _cts?.Cancel();
        _cts = null;
        ViewModel.Uninitialize();
    }
}
