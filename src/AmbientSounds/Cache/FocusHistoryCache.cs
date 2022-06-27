using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Cache
{
    public class FocusHistoryCache : IFocusHistoryCache
    {
        public async Task<IReadOnlyList<FocusHistory>> GetRecentAsync()
        {
            await Task.Delay(1);

            var list = new List<FocusHistory>
            {
                new FocusHistory
                {
                    StartUtcTicks = DateTime.UtcNow.AddDays(-1).Ticks,
                    FocusLength = 1,
                    RestLength = 1,
                    Repetitions = 0,
                    FocusSegmentsCompleted = 2,
                    RestSegmentsCompleted = 1,
                    PartialSegmentType = SessionType.Rest,
                    PartialSegmentTicks = TimeSpan.FromMinutes(5).Ticks
                },
                new FocusHistory
                {
                    StartUtcTicks = DateTime.UtcNow.AddDays(-9).Ticks,
                    FocusLength = 1,
                    RestLength = 1,
                    Repetitions = 8,
                    FocusSegmentsCompleted = 2,
                    RestSegmentsCompleted = 1,
                    PartialSegmentType = SessionType.Rest,
                    PartialSegmentTicks = TimeSpan.FromMinutes(5).Ticks
                },
                new FocusHistory
                {
                    StartUtcTicks = DateTime.UtcNow.AddDays(-2).Ticks,
                    FocusLength = 1,
                    RestLength = 1,
                    Repetitions = 1,
                    FocusSegmentsCompleted = 2,
                    RestSegmentsCompleted = 1,
                    PartialSegmentType = SessionType.Rest,
                    PartialSegmentTicks = TimeSpan.FromMinutes(5).Ticks
                },
            };

            return list.AsReadOnly();
        }
    }
}
