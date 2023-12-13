using AmbientSounds.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
        new PropertyMetadata(0, OnIndexChanged));

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

    private void UpdateCurrentTask(int newIndex)
    {
        if (ItemsSource is null ||
            newIndex < 0 ||
            newIndex >= ItemsSource.Count)
        {
            return;
        }

        var task = ItemsSource[newIndex];
        CurrentTaskText = task.Text;
        SelectedIndex = newIndex;
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
        FakeTaskPanel.Visibility = Visibility.Visible;
        TaskEntraceFromLeft.Start();
        await FakeTaskExitToRight.StartAsync();
        FakeTaskPanel.Visibility = Visibility.Collapsed;
    }
}
