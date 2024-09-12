using AmbientSounds.Constants;
using CommunityToolkit.Diagnostics;
using JeniusApps.Common.Settings;
using JeniusApps.Common.Telemetry;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public class PresenceService : IPresenceService
{
    private readonly HubConnection _connection;
    private readonly HashSet<string> _connectedSoundIds = [];
    private readonly string _deviceId; // used to uniquely identify this device in signalr.
    private readonly ITelemetry _telemetry;

    public event EventHandler<PresenceEventArgs>? SoundPresenceChanged;
    public event EventHandler? PresenceDisconnected;

    public PresenceService(
        IAppSettings appSettings,
        IUserSettings userSettings,
        ITelemetry telemetry)
    {
        Guard.IsNotNull(appSettings, nameof(appSettings));
        Guard.IsNotNull(userSettings, nameof(userSettings));
        Guard.IsNotNull(telemetry, nameof(telemetry));

        _telemetry = telemetry;

        _connection = new HubConnectionBuilder()
            .WithUrl(appSettings.PresenceUrl)
            .Build();

        _connection.On<string, double>("updatePresence", (s, d) =>
        {
            SoundPresenceChanged?.Invoke(this, new PresenceEventArgs(s, d));
        });

        _deviceId = GetOrCreateDeviceId(userSettings);
    }

    private static string GetOrCreateDeviceId(IUserSettings userSettings)
    {
        var deviceId = userSettings.Get<string>(UserSettingsConstants.DevicePresenceIdKey);
        if (string.IsNullOrEmpty(deviceId))
        {
            deviceId = Guid.NewGuid().ToString();
            userSettings.Set(UserSettingsConstants.DevicePresenceIdKey, deviceId);
        }
        return deviceId!;
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
        var ids = _connectedSoundIds.ToArray();
        foreach (var id in ids)
        {
            await DecrementAsync(id);
        }

        try
        {
            await _connection.StopAsync();
        }
        catch { }

        PresenceDisconnected?.Invoke(this, EventArgs.Empty);
    }

    public async Task IncrementAsync(string soundId)
    {
        if (_connection.State != HubConnectionState.Connected)
        {
            return;
        }

        try
        {
            await _connection.InvokeAsync("IncrementDevicePresence", _deviceId, soundId);
            _connectedSoundIds.Add(soundId);
        }
        catch (Exception e)
        {
            _telemetry.TrackError(e);
        }
    }

    public async Task DecrementAsync(string soundId)
    {
        if (_connection.State != HubConnectionState.Connected)
        {
            return;
        }

        try
        {
            await _connection.InvokeAsync("DecrementDevicePresence", _deviceId, soundId);
            _connectedSoundIds.Remove(soundId);
        }
        catch (Exception e)
        {
            _telemetry.TrackError(e);
        }
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
