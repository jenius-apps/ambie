using AmbientSounds.Factories;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public partial class CatalogueListViewModel : ObservableObject
    {
        private readonly IOnlineSoundDataProvider _dataProvider;
        private readonly ISoundVmFactory _soundVmFactory;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EmptyMessageVisible))]
        private bool _loading;

        public CatalogueListViewModel(
            IOnlineSoundDataProvider dataProvider,
            ISoundVmFactory soundVmFactory)
        {
            Guard.IsNotNull(dataProvider, nameof(dataProvider));
            Guard.IsNotNull(soundVmFactory, nameof(soundVmFactory));

            _dataProvider = dataProvider;
            _soundVmFactory = soundVmFactory;
        }

        public bool EmptyMessageVisible => !Loading && Sounds.Count == 0;

        /// <summary>
        /// The list of sounds for this page.
        /// </summary>
        public ObservableCollection<OnlineSoundViewModel> Sounds { get; } = new();

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (var s in Sounds)
            {
                s.Dispose();
            }

            Sounds.Clear();
        }

        public async Task InitializeAsync()
        {
            if (Sounds.Count > 0 || Loading)
            {
                return;
            }

            Loading = true;
            IList<Sound> sounds;

            try
            {
                sounds = await _dataProvider.GetSoundsAsync();
            }
            catch (Exception e)
            {
                // TODO log error
                Debug.WriteLine(e);
                Loading = false;
                return;
            }

            List<Task> tasks = new();

            foreach (var sound in sounds)
            {
                var vm = _soundVmFactory.GetOnlineSoundVm(sound);
                if (vm is not null)
                {
                    tasks.Add(vm.LoadCommand.ExecuteAsync(null));
                    Sounds.Add(vm);
                }
            }

            await Task.WhenAll(tasks);

            Loading = false;
        }
    }
}
