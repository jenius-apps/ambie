using AmbientSounds.ViewModels;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class TaskTicker : ObservableUserControl
{
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource),
        typeof(ObservableCollection<FocusTaskViewModel>),
        typeof(TaskTicker),
        new PropertyMetadata(null, OnItemsSourceChanged));

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaskTicker s)
        {
            s.UpdateCurrentTask(0);
        }
    }

    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
        nameof(SelectedIndex),
        typeof(int),
        typeof(TaskTicker),
        new PropertyMetadata(-1, OnIndexChanged));

    private static void OnIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaskTicker s && e.NewValue is int newVal)
        {
            s.UpdateCurrentTask(newVal);
        }
    }

    public static readonly DependencyProperty CurrentTaskTextProperty = DependencyProperty.Register(
        nameof(CurrentTaskText),
        typeof(string),
        typeof(TaskTicker),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty CurrentTaskCompletedProperty = DependencyProperty.Register(
        nameof(CurrentTaskCompleted),
        typeof(bool),
        typeof(TaskTicker),
        new PropertyMetadata(false));

    public TaskTicker()
    {
        this.InitializeComponent();
    }

    public ObservableCollection<FocusTaskViewModel>? ItemsSource
    {
        get => (ObservableCollection<FocusTaskViewModel>)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValueDp(SelectedIndexProperty, value);
    }

    public string? CurrentTaskText
    {
        get => (string?)GetValue(CurrentTaskTextProperty);
        set => SetValue(CurrentTaskTextProperty, value);
    }

    public bool CurrentTaskCompleted
    {
        get => (bool)GetValue(CurrentTaskCompletedProperty);
        set => SetValue(CurrentTaskCompletedProperty, value);
    }

    private void UpdateCurrentTask(int newIndex)
    {
        if (ItemsSource is null ||
            newIndex == SelectedIndex ||
            newIndex < 0 ||
            newIndex >= ItemsSource.Count)
        {
            return;
        }
        
        SelectedIndex = newIndex;
        var task = ItemsSource[newIndex];
        CurrentTaskText = task.Text;
        CurrentTaskCompleted = task.IsCompleted;
        
        // Note: this must come after the SelectedIndex = newIndex line
        // to avoid a race condition whereby the OnUnchecked will get triggered
        // but the operation below, and that would use the incorrect
        // SelectedIndex.
        RealCheckBox.IsChecked = CurrentTaskCompleted;
    }

    private async void Next(object sender, RoutedEventArgs e)
    {
        if (ItemsSource is null || 
            ItemsSource.Count == 0 || 
            SelectedIndex + 1 >= ItemsSource.Count)
        {
            return;
        }

        var oldTask = ItemsSource[SelectedIndex];
        UpdateCurrentTask(SelectedIndex + 1);

        FakeTaskTextBlock.Text = oldTask.Text;
        FakeCheckBox.IsChecked = oldTask.IsCompleted;
        FakeTaskPanel.Visibility = Visibility.Visible;
        TaskEntraceFromRight.Start();
        await FakeTaskExitToLeft.StartAsync();
        FakeTaskPanel.Visibility = Visibility.Collapsed;
    }

    private async void Previous(object sender, RoutedEventArgs e)
    {
        if (ItemsSource is null ||
            ItemsSource.Count == 0 ||
            SelectedIndex - 1 < 0)
        {
            return;
        }

        var oldTask = ItemsSource[SelectedIndex];
        UpdateCurrentTask(SelectedIndex - 1);

        FakeTaskTextBlock.Text = oldTask.Text;
        FakeCheckBox.IsChecked = oldTask.IsCompleted;
        FakeTaskPanel.Visibility = Visibility.Visible;
        TaskEntraceFromLeft.Start();
        await FakeTaskExitToRight.StartAsync();
        FakeTaskPanel.Visibility = Visibility.Collapsed;
    }

    private void OnChecked(object sender, RoutedEventArgs e)
    {
        if (ItemsSource is { } source && 
            source[SelectedIndex] is { IsCompleted: false } task)
        {
            task.IsCompleted = true;
            Next(sender, e);
        }
    }

    private void OnUnchecked(object sender, RoutedEventArgs e)
    {
        if (ItemsSource is { } source &&
            source[SelectedIndex] is { IsCompleted: true } task)
        {
            task.IsCompleted = false;
        }
    }
}
