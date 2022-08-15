﻿using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls
{
    public sealed partial class FocusHistoryModule : UserControl, ICanUninitialize
    {
        public FocusHistoryModule()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<FocusHistoryModuleViewModel>();
        }

        public FocusHistoryModuleViewModel ViewModel => (FocusHistoryModuleViewModel)this.DataContext;

        public void Uninitialize()
        {
            ViewModel.Uninitialize();
        }

        private void OnHistoryClicked(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is FocusHistoryViewModel vm)
            {
                ViewModel.DetailsCommand.Execute(vm);
            }
        }
    }
}
