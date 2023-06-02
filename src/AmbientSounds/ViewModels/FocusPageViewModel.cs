using AmbientSounds.Services;
using JeniusApps.Common.Tools;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class FocusPageViewModel : ObservableObject
{
    private readonly IFocusNotesService _focusNotesService;
    private readonly IFocusService _focusService;
    private readonly IDispatcherQueue _dispatcherQueue;

    [ObservableProperty]
    private string _notes = string.Empty;

    public FocusPageViewModel(
        IFocusNotesService focusNotesService,
        IFocusService focusService,
        IDispatcherQueue dispatcherQueue)
    {
        Guard.IsNotNull(focusNotesService);
        Guard.IsNotNull(focusService);
        Guard.IsNotNull(dispatcherQueue);

        _focusNotesService = focusNotesService;
        _focusService = focusService;
        _dispatcherQueue = dispatcherQueue;
    }

    /// <summary>
    /// Determines if the task module is visible.
    /// </summary>
    /// <remarks>
    /// With the introduction of the timer module's new
    /// Focused Task feature, we hide the task module to prevent 
    /// tasks from being edited during a focus session. This avoids
    /// the need to synchronize the data in real-time.
    /// </remarks>
    public bool TaskModuleVisible => _focusService.CurrentState == FocusState.None;

    /// <inheritdoc/>
    partial void OnNotesChanged(string value)
    {
        _focusNotesService.UpdateNotes(value);
    }

    public async Task InitializeAsync()
    {
        _focusService.FocusStateChanged += OnFocusStateChanged;

        Notes = await _focusNotesService.GetStoredNotesAsync();
    }

    public void Uninitialize()
    {
        _focusService.FocusStateChanged -= OnFocusStateChanged;
    }

    public Task SaveNotesToStorageAsync()
    {
        return _focusNotesService.SaveNotesToStorageAsync();
    }

    private void OnFocusStateChanged(object sender, FocusState e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            OnPropertyChanged(nameof(TaskModuleVisible));
        });
    }
}
