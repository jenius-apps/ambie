using AmbientSounds.Models;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class CatalogueRowControl : UserControl
{
    private const double ScrollOffset = 100d;
    private ScrollViewer? _scrollViewer;

    public static readonly DependencyProperty RowDataProperty = DependencyProperty.Register(
        nameof(RowData),
        typeof(CatalogueRow),
        typeof(CatalogueRowControl), 
        new PropertyMetadata(null, OnRowDataChanged));

    public CatalogueRowControl()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<CatalogueRowViewModel>();
    }

    public CatalogueRowViewModel ViewModel { get; }

    public CatalogueRow? RowData
    {
        get => (CatalogueRow)GetValue(RowDataProperty);
        set => SetValue(RowDataProperty, value);
    }

    private static async void OnRowDataChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        if (d is CatalogueRowControl c && e.NewValue is CatalogueRow row)
        {
            await c.ViewModel.LoadAsync(row);
        }
    }

    private void OnRightClicked(object sender, RoutedEventArgs e)
    {
        _scrollViewer ??= FindVisualChild<ScrollViewer>(SoundListView);
        _scrollViewer?.ChangeView(_scrollViewer.HorizontalOffset + ScrollOffset, null, null);
    }

    private void OnLeftClicked(object sender, RoutedEventArgs e)
    {
        _scrollViewer ??= FindVisualChild<ScrollViewer>(SoundListView);
        _scrollViewer?.ChangeView(_scrollViewer.HorizontalOffset - ScrollOffset, null, null);
    }

    private static T? FindVisualChild<T>(DependencyObject? obj) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(obj, i);

            if (child != null && child is T)
            {
                return (T)child;
            }
            else
            {
                T? childOfChild = FindVisualChild<T>(child);

                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }
        }

        return null;
    }
}
