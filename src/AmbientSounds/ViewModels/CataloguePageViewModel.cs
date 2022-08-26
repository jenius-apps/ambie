﻿using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AmbientSounds.ViewModels
{
    /// <summary>
    /// ViewModel representing the catalogue page.
    /// </summary>
    public class CataloguePageViewModel : ObservableObject
    {
        private readonly INavigator _navigator;

        public CataloguePageViewModel(INavigator navigator)
        {
            Guard.IsNotNull(navigator, nameof(navigator));

            _navigator = navigator;
        }

        /// <summary>
        /// Navigates the frame backwards.
        /// </summary>
        public void GoBack() => _navigator.GoBack();
    }
}
