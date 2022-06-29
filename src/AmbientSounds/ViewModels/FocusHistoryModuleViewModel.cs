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
        private bool _loading;
        private bool _placeholderVisible;

        public FocusHistoryModuleViewModel(IFocusHistoryService focusHistoryService)
        {
            Guard.IsNotNull(focusHistoryService, nameof(focusHistoryService));

            _focusHistoryService = focusHistoryService;
        }

        public ObservableCollection<FocusHistoryViewModel> Items { get; } = new();

        public bool Loading
        {
            get => _loading;
            set => SetProperty(ref _loading, value);
        }

        public bool PlaceholderVisible
        {
            get => _placeholderVisible;
            set => SetProperty(ref _placeholderVisible, value);
        }

        public async Task InitializeAsync()
        {
            _focusHistoryService.HistoryAdded += OnHistoryAdded;
            Items.Clear();

            Loading = true;
            var recent = await _focusHistoryService.GetRecentAsync();
            await Task.Delay(300);
            Loading = false;
            foreach (var focusHistory in recent.OrderByDescending(x => x.StartUtcTicks))
            {
                Items.Add(new FocusHistoryViewModel(focusHistory));
            }

            UpdatePlaceholder();
        }

        public void Uninitialize()
        {
            Items.Clear();
            _focusHistoryService.HistoryAdded -= OnHistoryAdded;
        }

        private void UpdatePlaceholder() => PlaceholderVisible = Items.Count == 0;

        private void OnHistoryAdded(object sender, FocusHistory? history)
        {
            if (history is FocusHistory f)
            {
                Items.Insert(0, new FocusHistoryViewModel(f));
                UpdatePlaceholder();
            }
        }
    }
}
