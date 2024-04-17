using AmbientSounds.ViewModels;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class XboxSoundItem : UserControl
{
    private readonly SemaphoreSlim _namePlateLock = new(1, 1);
    private CancellationTokenSource _namePlateAnimationCts = new();

    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(SoundViewModel),
        typeof(SoundItemControl),
        new PropertyMetadata(null, OnViewModelChanged));

    public XboxSoundItem()
    {
        this.InitializeComponent();
    }

    public SoundViewModel? ViewModel
    {
        get => (SoundViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    private void RegisterViewModel(SoundViewModel newVm)
    {
        if (ViewModel is { } oldVm)
        {
            oldVm.PropertyChanged -= OnPropertyChanged;
        }

        newVm.PropertyChanged += OnPropertyChanged;
    }

    private async void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        if (e.PropertyName == nameof(SoundViewModel.IsKeyPadFocused))
        {
            _namePlateAnimationCts.Cancel();
            _namePlateAnimationCts = new();

            if (ViewModel.IsKeyPadFocused)
            {
                await _namePlateLock.WaitAsync();

                NamePlate.Visibility = Visibility.Visible;
                try
                {
                    await FadeInName.StartAsync(_namePlateAnimationCts.Token);
                }
                catch (OperationCanceledException)
                {

                }

                _namePlateLock.Release();
            }
            else
            {
                await _namePlateLock.WaitAsync();

                try
                {
                    await FadeOutName.StartAsync(_namePlateAnimationCts.Token);
                }
                catch (OperationCanceledException)
                {

                }

                NamePlate.Visibility = Visibility.Collapsed;
                _namePlateLock.Release();
            }
        }
    }

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is SoundViewModel newVm)
        {
            ((XboxSoundItem)d).RegisterViewModel(newVm);
        }
    }
}
