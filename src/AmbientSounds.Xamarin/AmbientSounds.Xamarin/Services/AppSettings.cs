using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.Services.Xamarin
{
    public class AppSettings : IAppSettings
    {
        public string MySoundsUrl { get; }
        public string CatalogueUrl { get; }
        public string TelemetryApiKey { get; }
        public bool LoadPreviousState { get; set; }
        public string MsaClientId { get; set; }
        public string CloudSyncFileUrl { get; set; }
        public string UploadUrl { get; set; }
        public string CatalogueScope { get; set; }
    }
}
