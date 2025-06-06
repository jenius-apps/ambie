﻿using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace AmbientSounds.ViewModels;

public partial class DayActivityViewModel : ObservableObject
{
    public required bool Active { get; init; }

    public DateTime Date { get; init; }

    public string DayOfWeekShort => Date.DayOfWeek.ToString()[..2];
}
