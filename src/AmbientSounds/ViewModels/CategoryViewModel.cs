using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AmbientSounds.ViewModels;

public partial class CategoryViewModel : ObservableObject
{
    public CategoryViewModel(
        Category category,
        IAssetLocalizer assetLocalizer)
    {
        Model = category;
        Name = assetLocalizer.GetLocalInfo(category)?.Name ?? category.Id;
    }

    /// <summary>
    /// The core model.
    /// </summary>
    public Category Model { get; }

    /// <summary>
    /// The localized name of this category.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Determines if the viewmodel is currently selected.
    /// </summary>
    [ObservableProperty]
    private bool _isSelected;
}
