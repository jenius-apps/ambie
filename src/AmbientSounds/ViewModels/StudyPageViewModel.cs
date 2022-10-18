using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public partial class StudyPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private StudyPageStatus _currentPage;

        [RelayCommand]
        private async Task CreateRoomAsync()
        {
            await Task.Delay(1);
            CurrentPage = StudyPageStatus.Room;
        }
    }

    public enum StudyPageStatus
    {
        Home,
        Room
    }
}
