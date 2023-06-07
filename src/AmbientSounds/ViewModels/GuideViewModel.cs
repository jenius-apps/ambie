using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class GuideViewModel : ObservableObject
{
    private readonly Guide _guide;

    public GuideViewModel(
        Guide guide,
        IAssetLocalizer assetLocalizer)
    {
        _guide = guide;
        Name = assetLocalizer.GetLocalName(_guide);
    }

    public string Name { get; }

    public string PreviewText { get; } = "This is a preview";

    public string ImagePath { get; } = "https://images.unsplash.com/photo-1617354161552-cce2e5b27f79?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=640&q=80";
}
