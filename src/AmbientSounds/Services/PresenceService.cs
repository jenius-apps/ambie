using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace AmbientSounds.Services
{
    public class PresenceService : IPresenceService
    {
        private readonly HubConnection _connection;
        public event EventHandler<PresenceEventArgs>? SoundPresenceChanged;

        public PresenceService(IAppSettings appSettings)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(appSettings.PresenceUrl)
                .Build();

            _connection.On<string, double>("updatePresence", (s, d) =>
            {
                SoundPresenceChanged?.Invoke(this, new PresenceEventArgs(s, d));
            });
        }

        public async Task EnsureInitializedAsync()
        {
            if (_connection.State != HubConnectionState.Disconnected)
            {
                return;
            }

            try
            {
                await _connection.StartAsync();
            }
            catch (Exception) 
            {
                // Note: if debugging with localhost, ensure the server code has
                // apps.UseHttpsRedirection() commented out in Program.cs.
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                await _connection.StopAsync();
            }
            catch { }
        }

        public async Task IncrementAsync(string soundId)
        {
            if (_connection.State != HubConnectionState.Connected)
            {
                return;
            }

            try
            {
                await _connection.InvokeAsync("IncrementPresence", soundId);
            }
            catch { }
        }

        public async Task DecrementAsync(string soundId)
        {
            if (_connection.State != HubConnectionState.Connected)
            {
                return;
            }

            try
            {
                await _connection.InvokeAsync("DecrementPresence", soundId);
            }
            catch { }
        }
    }

    public class PresenceEventArgs : EventArgs
    {
        public PresenceEventArgs(string soundId, double count)
        {
            SoundId = soundId ?? string.Empty;
            Count = count >= 0 ? count : 0;
        }

        public string SoundId { get; }

        public double Count { get; }
    }
}
