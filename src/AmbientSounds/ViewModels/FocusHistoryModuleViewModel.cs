using AmbientSounds.Models;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class FocusHistoryModuleViewModel : ObservableObject
    {
        private readonly IFocusHistoryService _focusHistoryService;

        public FocusHistoryModuleViewModel(IFocusHistoryService focusHistoryService)
        {
            Guard.IsNotNull(focusHistoryService, nameof(focusHistoryService));

            _focusHistoryService = focusHistoryService;
        }

        public ObservableCollection<FocusHistoryViewModel> Items { get; } = new();

        public async Task InitializeAsync()
        {
            _focusHistoryService.HistoryAdded += OnHistoryAdded;
            Items.Clear();
            var recent = await _focusHistoryService.GetRecentAsync();
            foreach (var focusHistory in recent.OrderByDescending(x => x.StartUtcTicks))
            {
                Items.Add(new FocusHistoryViewModel(focusHistory));
            }
        }

        public void Uninitialize()
        {
            Items.Clear();
            _focusHistoryService.HistoryAdded -= OnHistoryAdded;
        }

        private void OnHistoryAdded(object sender, FocusHistory? history)
        {
            if (history is FocusHistory f)
            {
                Items.Insert(0, new FocusHistoryViewModel(f));
            }
        }
    }
}
