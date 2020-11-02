using AmbientSounds.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class MainPageViewModel : ObservableObject
    {
        private readonly MediaPlayerService _player;

        /// <summary>
        /// Default constructor. Must initialize with <see cref="LoadAsync"/>
        /// immediately after creation.
        /// </summary>
        public MainPageViewModel(MediaPlayerService mediaPlayerService)
        {
            _player = mediaPlayerService ?? throw new ArgumentNullException(nameof(mediaPlayerService));

            LoadCommand = new AsyncRelayCommand(LoadAsync);
        }

        /// <summary>
        /// The <see cref="IAsyncRelayCommand"/> responsible for loading the viewmodel data.
        /// </summary>
        public IAsyncRelayCommand LoadCommand { get; }

        /// <summary>
        /// The list of sounds for this page.
        /// </summary>
        public ObservableCollection<SoundViewModel> Sounds { get; } = new ObservableCollection<SoundViewModel>();

        /// <summary>
        /// Loads the list of sounds for this view model.
        /// </summary>
        private async Task LoadAsync()
        {
            var soundList = await SoundDataProvider.GetSoundsAsync();

            foreach (var sound in soundList)
            {
                Sounds.Add(new SoundViewModel(sound, _player));
            }
        }

        /// <summary>
        /// Loads the clicked sound into the player and plays it.
        /// </summary>
        public void GridViewSoundClicked(object sender, Windows.UI.Xaml.Controls.ItemClickEventArgs e)
        {
            if (e.ClickedItem is SoundViewModel svm)
            {
                svm.Play();
            }
        }
    }
}
