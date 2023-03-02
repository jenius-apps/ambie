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
}
