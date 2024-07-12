using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class ChannelsPageViewModel : ObservableObject
{
    public ChannelsPageViewModel()
    {

    }

    public async Task InitializeAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        await Task.Delay(1);

    }

    public void Uninitialize()
    {

    }
}
