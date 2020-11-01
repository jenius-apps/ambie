using AmbientSounds.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
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
        }

        /// <summary>
        /// The list of sounds for this page.
        /// </summary>
        public ObservableCollection<SoundViewModel> Sounds { get; } = new ObservableCollection<SoundViewModel>();

        /// <summary>
        /// Flag for loading the list of sounds.
        /// </summary>
        public bool Loading
        {
            get => _loading;
            set => SetProperty(ref _loading, value);
        }
        private bool _loading;

        /// <summary>
        /// Loads the list of sounds for this view model.
        /// </summary>
        public async void LoadAsync()
        {
            Loading = true;

            var soundList = await SoundDataProvider.GetSoundsAsync();
            foreach (var sound in soundList)
            {
                Sounds.Add(new SoundViewModel(sound, _player));
            }

            Loading = false;
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
