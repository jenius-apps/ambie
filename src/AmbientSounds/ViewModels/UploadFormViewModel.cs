using AmbientSounds.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class UploadFormViewModel : ObservableObject
    {
        private string _name = "";
        private string _attribution = "";
        private string _imageUrl = "";
        private string _soundPath = "";

        public UploadFormViewModel()
        {
            SubmitCommand = new AsyncRelayCommand(SubmitAsync);
        }

        public IAsyncRelayCommand SubmitCommand { get; }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Attribution
        {
            get => _attribution;
            set => SetProperty(ref _attribution, value);
        }

        public string ImageUrl
        {
            get => _imageUrl;
            set => SetProperty(ref _imageUrl, value);
        }

        public string SoundPath
        {
            get => _soundPath;
            set => SetProperty(ref _soundPath, value);
        }

        private async Task SubmitAsync()
        {
            var s = new Sound
            {
                Name = Name,
                Attribution = Attribution,
                ImagePath = ImageUrl,
                FilePath = SoundPath,
                FileExtension = ".mp3" // TODO use nuget package to determine extension of file
            };

            await Task.Delay(1);

            // upload
        }
    }
}
