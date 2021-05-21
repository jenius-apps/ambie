using AmbientSounds.Constants;
using AmbientSounds.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls
{
    public sealed partial class ShareMixButton : UserControl
    {
        public ShareMixButton()
        {
            this.InitializeComponent();
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            this.Unloaded += (_, _) => { dataTransferManager.DataRequested -= DataTransferManager_DataRequested; };

        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            string link = App.Services.GetRequiredService<IShareLinkBuilder>().GetLink();

            DataRequest request = args.Request;
            request.Data.SetWebLink(new Uri(link));
            request.Data.Properties.Title = link;
            request.Data.Properties.Description = "Sound mix deep link";
        }

        private void OpenShareMenu(object sender, RoutedEventArgs e)
        {
            var telemetry = App.Services.GetRequiredService<ITelemetry>();
            telemetry.TrackEvent(TelemetryConstants.ShareClicked);
            DataTransferManager.ShowShareUI();
        }
    }
}
