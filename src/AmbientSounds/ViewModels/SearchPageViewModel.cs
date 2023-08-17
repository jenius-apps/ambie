﻿using AmbientSounds.Factories;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using JeniusApps.Common.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public sealed partial class SearchPageViewModel : ObservableObject
{
    private CancellationTokenSource? _cts;
    private readonly ISoundVmFactory _vmFactory;
    private readonly ISearchService _searchService;
    private readonly ILocalizer _localizer;

    public SearchPageViewModel(
        ISoundVmFactory vmFactory,
        ISearchService searchService,
        ILocalizer localizer)
    {
        _vmFactory = vmFactory;
        _searchService = searchService;
        _localizer = localizer;
    }

    /// <summary>
    /// List of sounds that are the result of the search operation.
    /// </summary>
    public ObservableCollection<OnlineSoundViewModel> Sounds { get; } = new();

    /// <summary>
    /// Header text for the search results.
    /// </summary>
    [ObservableProperty]
    private string _headerText = string.Empty;

    /// <summary>
    /// Initializes the vm. Should always be run on page-to navigation.
    /// </summary>
    public void Initialize()
    {
        _searchService.ModifyCurrentSearchRequested += OnModifyCurrentSearchRequested;
    }

    /// <summary>
    /// Uinitializes the vm. Should always be run on page-from navigation.
    /// </summary>
    public void Uninitialize()
    {
        _cts?.Cancel();
        _searchService.ModifyCurrentSearchRequested -= OnModifyCurrentSearchRequested;

        foreach (var s in Sounds)
        {
            s.Dispose();
        }

        Sounds.Clear();
    }

    /// <summary>
    /// Entry point in triggering a search.
    /// </summary>
    /// <param name="name">The name to query.</param>
    public async Task TriggerSearchAsync(string name)
    {
        _cts?.Cancel();
        _cts = new();

        try
        {
            await SearchAsync(name, _cts.Token);
        }
        catch (OperationCanceledException) { }
    }

    /// <summary>
    /// Internal method that performs search and updates the UI
    /// based on the results.
    /// </summary>
    /// <param name="name">The name to query.</param>
    /// <param name="ct">A token for cancellation.</param>
    private async Task SearchAsync(string name, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        HeaderText = _localizer.GetString("SearchPageHeader", name);

        Sounds.Clear();
        IReadOnlyList<Sound> results = await _searchService.SearchByNameAsync(name);
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
            ct.ThrowIfCancellationRequested();
            Sounds.Add(vm);
        }
    }

    private async void OnModifyCurrentSearchRequested(object sender, string e)
    {
        await TriggerSearchAsync(e);
    }
}
