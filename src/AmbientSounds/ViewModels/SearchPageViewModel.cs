﻿using AmbientSounds.Factories;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public sealed partial class SearchPageViewModel : ObservableObject
{
    private readonly ISoundVmFactory _vmFactory;
    private readonly ISearchService _searchService;

    public SearchPageViewModel(
        ISoundVmFactory vmFactory,
        ISearchService searchService)
    {
        _vmFactory = vmFactory;
        _searchService = searchService;
    }

    public ObservableCollection<OnlineSoundViewModel> Sounds { get; } = new();

    [ObservableProperty]
    private string _headerText = string.Empty;

    public async Task SearchAsync(string query, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        HeaderText = $"Results for \"{query}\"";

        Sounds.Clear();
        IReadOnlyList<Sound> results = await _searchService.SearchByNameAsync(query);
        ct.ThrowIfCancellationRequested();

        if (results.Count == 0)
        {
            return;
        }

        foreach (var sound in results)
        {
            ct.ThrowIfCancellationRequested();
            var vm = _vmFactory.GetOnlineSoundVm(sound);
            if (vm is null)
            {
                continue;
            }

            await vm.LoadCommand.ExecuteAsync(null);
            Sounds.Add(vm);
        }
    }
}