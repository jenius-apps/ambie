using AmbientSounds.Models;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AmbientSounds.ViewModels;

public partial class FocusTaskViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isCompleted;

    [ObservableProperty]
    private string _text = string.Empty;

    public FocusTaskViewModel(
        FocusTask task,
        IRelayCommand<FocusTaskViewModel>? delete = null,
        IRelayCommand<FocusTaskViewModel>? edit = null,
        IRelayCommand<FocusTaskViewModel>? complete = null,
        IRelayCommand<FocusTaskViewModel>? reopen = null,
        string? displayTitle = null)
    {
        Guard.IsNotNull(task, nameof(task));
        Task = task;
        DisplayTitle = displayTitle ?? string.Empty;
        _isCompleted = task.Completed;
        Text = task.Text;
        CompleteCommand = complete;
        ReopenCommand = reopen;

        // a fallback is used for these because they might be used with xaml binding.
        // So we ensure that if it is bound, it's not null.
        EditCommand = edit ?? new RelayCommand<FocusTaskViewModel>(vm => { });
        DeleteCommand = delete ?? new RelayCommand<FocusTaskViewModel>(vm => { }); 
    }

    public FocusTask Task { get; }

    public string DisplayTitle { get; }

    public IRelayCommand<FocusTaskViewModel> EditCommand { get; }

    public IRelayCommand<FocusTaskViewModel> DeleteCommand { get; }

    public IRelayCommand<FocusTaskViewModel>? CompleteCommand { get; }

    public IRelayCommand<FocusTaskViewModel>? ReopenCommand { get; }

    /// <inheritdoc/>
    partial void OnIsCompletedChanged(bool value)
    {
        if (value is true)
        {
            CompleteCommand?.Execute(this);
        }
        else
        {
            ReopenCommand?.Execute(this);
        }
    }

    public override string ToString()
    {
        return string.IsNullOrEmpty(DisplayTitle)
            ? Text
            : $"{DisplayTitle}. {Text}.";
    }
}
