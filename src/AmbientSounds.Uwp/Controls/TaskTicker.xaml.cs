﻿using AmbientSounds.ViewModels;
using System;
using System.Collections.ObjectModel;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class TaskTicker : ObservableUserControl
{
    public event EventHandler<string>? AddNewTaskRequested;

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource),
        typeof(ObservableCollection<FocusTaskViewModel>),
        typeof(TaskTicker),
        new PropertyMetadata(null, OnItemsSourceChanged));

    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
        nameof(SelectedIndex),
        typeof(int),
        typeof(TaskTicker),
        new PropertyMetadata(-1, OnIndexChanged));

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

    public static readonly DependencyProperty NewTaskInputProperty = DependencyProperty.Register(
        nameof(NewTaskInput),
        typeof(string),
        typeof(TaskTicker),
        new PropertyMetadata(string.Empty));

    private static readonly DependencyProperty NewTaskButtonVisibleProperty = DependencyProperty.Register(
        nameof(NewTaskButtonVisible),
        typeof(bool),
        typeof(TaskTicker),
        new PropertyMetadata(false));

    private static readonly DependencyProperty NewTaskPanelVisibleProperty = DependencyProperty.Register(
        nameof(NewTaskPanelVisible),
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

    public string NewTaskInput
    {
        get => (string)GetValue(NewTaskInputProperty);
        set => SetValue(NewTaskInputProperty, value);
    }

    private bool NewTaskButtonVisible
    {
        get => (bool)GetValue(NewTaskButtonVisibleProperty);
        set => SetValue(NewTaskButtonVisibleProperty, value);
    }

    private bool NewTaskPanelVisible
    {
        get => (bool)GetValue(NewTaskPanelVisibleProperty);
        set => SetValue(NewTaskPanelVisibleProperty, value);
    }

    private void UpdateCurrentTask(int newIndex)
    {
        if (ItemsSource is null ||
            newIndex < 0 ||
            newIndex >= ItemsSource.Count)
        {
            return;
        }
        
        SelectedIndex = newIndex;
        var task = ItemsSource[newIndex];
        CurrentTaskText = task.Text;
        CurrentTaskCompleted = task.IsCompleted;
        NewTaskButtonVisible = SelectedIndex == ItemsSource.Count - 1;

        // Note: this must come after the SelectedIndex = newIndex line
        // to avoid a race condition whereby OnUnchecked will get triggered
        // by the operation below, and that would use the incorrect SelectedIndex.
        RealCheckBox.IsChecked = CurrentTaskCompleted;
    }

    private async void Next(object sender, RoutedEventArgs e)
    {
        if (ItemsSource is null || 
            ItemsSource.Count == 0)
        {
            return;
        }

        if (NewTaskButtonVisible)
        {
            NewTaskPanelVisible = true;
            InputTextBox.Focus(FocusState.Programmatic);
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

            if (SelectedIndex < source.Count - 1)
            {
                Next(sender, e);
            }
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

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaskTicker s)
        {
            s.UpdateCurrentTask(0);
        }
    }

    private static void OnIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaskTicker s && e.NewValue is int newVal)
        {
            s.UpdateCurrentTask(newVal);
        }
    }

    private void RaiseNewTaskEvent()
    {
        if (!string.IsNullOrWhiteSpace(NewTaskInput))
        {
            NewTaskPanelVisible = false;
            AddNewTaskRequested?.Invoke(this, NewTaskInput);
            NewTaskInput = string.Empty;
        }
    }

    private void CancelNewTask()
    {
        NewTaskPanelVisible = false;
        NewTaskInput = string.Empty;
    }

    private void OnInputKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            e.Handled = true;
            RaiseNewTaskEvent();
        }
        else if (e.Key is VirtualKey.Escape)
        {
            e.Handled = true;
            CancelNewTask();
        }
    }

    private void OnCancelAddTask(object sender, RoutedEventArgs e)
    {
        CancelNewTask();
    }

    private void OnSubmitNewTask(object sender, RoutedEventArgs e)
    {
        RaiseNewTaskEvent();
    }
}
