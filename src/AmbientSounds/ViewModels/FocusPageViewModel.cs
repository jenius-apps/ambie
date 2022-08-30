using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class FocusPageViewModel : ObservableObject
    {
        private readonly IFocusNotesService _focusNotesService;
        private readonly IFocusService _focusService;
        private string _notes = string.Empty;

        public FocusPageViewModel(
            IFocusNotesService focusNotesService,
            IFocusService focusService)
        {
            Guard.IsNotNull(focusNotesService);
            _focusNotesService = focusNotesService;
            Guard.IsNotNull(focusService);
            _focusService = focusService;
        }

        /// <summary>
        /// Determines if the task module is visible.
        /// </summary>
        /// <remarks>
        /// With the introduction of the timer module's new
        /// Focused Task feature, we hide the task module to prevent 
        /// tasks from being edited during a focus session. This avoids
        /// the need to synchronize the data in real-time.
        /// </remarks>
        public bool TaskModuleVisible => _focusService.CurrentState == FocusState.None;

        public string Notes
        {
            get => _notes;
            set
            {
                if (SetProperty(ref _notes, value))
                {
                    _focusNotesService.UpdateNotes(value);
                }
            }
        }

        public async Task InitializeAsync()
        {
            _focusService.FocusStateChanged += OnFocusStateChanged;
            _notes = await _focusNotesService.GetStoredNotesAsync();
            OnPropertyChanged(nameof(Notes));
        }

        public void Uninitialize()
        {
            _focusService.FocusStateChanged -= OnFocusStateChanged;
        }

        public Task SaveNotesToStorageAsync()
        {
            return _focusNotesService.SaveNotesToStorageAsync();
        }

        private void OnFocusStateChanged(object sender, FocusState e)
        {
            OnPropertyChanged(nameof(TaskModuleVisible));
        }
    }
}
