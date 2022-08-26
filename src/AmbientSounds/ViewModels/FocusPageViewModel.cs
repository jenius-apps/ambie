using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class FocusPageViewModel : ObservableObject
    {
        private readonly IFocusNotesService _focusNotesService;
        private string _notes = string.Empty;

        public FocusPageViewModel(
            IFocusNotesService focusNotesService)
        {
            Guard.IsNotNull(focusNotesService, nameof(focusNotesService));
            _focusNotesService = focusNotesService;
        }

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
            _notes = await _focusNotesService.GetStoredNotesAsync();
            OnPropertyChanged(nameof(Notes));
        }


        public void Uninitialize()
        {
        }

        public Task SaveNotesToStorageAsync()
        {
            return _focusNotesService.SaveNotesToStorageAsync();
        }
    }
}
