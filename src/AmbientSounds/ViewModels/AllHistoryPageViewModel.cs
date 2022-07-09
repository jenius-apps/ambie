using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class AllHistoryPageViewModel : ObservableObject
    {
        private readonly IFocusHistoryService _focusHistoryService;

        public AllHistoryPageViewModel(IFocusHistoryService focusHistoryService)
        {
            Guard.IsNotNull(focusHistoryService, nameof(focusHistoryService));

            _focusHistoryService = focusHistoryService;
        }

        public ObservableCollection<FocusHistoryViewModel> Items { get; } = new();

        public async Task InitializeAsync()
        {
            var recents = await _focusHistoryService.GetRecentAsync();
            foreach (var r in recents)
            {
                Items.Add(new FocusHistoryViewModel(r));
            }
        }
    }
}
