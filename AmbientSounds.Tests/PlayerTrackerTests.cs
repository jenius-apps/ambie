using System;
using Xunit;
using AmbientSounds.Services;
using Moq;
using JeniusApps.Common.Telemetry;

namespace AmbientSounds.Tests
{
    public class PlayerTrackerTests
    {
        [Theory]
        [InlineData(-1, "")] // invalid data
        [InlineData(30, "<1 min")] // 30 seconds
        [InlineData(180, "<5 min")] // 3 minutes
        [InlineData(480, "<10 min")] // 8 minutes
        [InlineData(2580, "40 min")] // 43 minutes
        [InlineData(3600, "60 min")] // 60 minutes
        [InlineData(3601, "1 hrs")] // 1.0003 hours
        [InlineData(28440, "8 hrs")] // 7.9 hours
        [InlineData(169200, "47 hrs")] // 47 hours
        [InlineData(176400, ">48 hrs")] // 49 hours
        public void RoundedDiffTests(double diffInSeconds, string expectedResult)
        {
            TimeSpan diff = TimeSpan.FromSeconds(diffInSeconds);
            var result = PlayerTelemetryTracker.GetRoundedDiff(diff);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void TrackDurationTest()
        {
            var playerMock = new Mock<IMixMediaPlayerService>();
            var telemetryMock = new Mock<ITelemetry>();

            var tracker = new PlayerTelemetryTracker(playerMock.Object, telemetryMock.Object);
            string result = tracker.TrackDuration(DateTimeOffset.Now);

            Assert.Equal(string.Empty, result);
        }
    }
}
