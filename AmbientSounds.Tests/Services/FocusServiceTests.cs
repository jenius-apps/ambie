using AmbientSounds.Services;
using AmbientSounds.Tools;
using JeniusApps.Common.Settings;
using JeniusApps.Common.Telemetry;
using Moq;
using Xunit;

namespace AmbientSounds.Tests.Services;

public class FocusServiceTests
{
    [Fact]
    public void PostFocusVolumeTest()
    {
        // Arrange
        double previousVolume = 0.5d;
        var playerMock = new Mock<IMixMediaPlayerService>();
        _ = playerMock.SetupProperty(x => x.GlobalVolume, MixMediaPlayerService.EffectiveMuteVolume);

        var focusService = new FocusService(
            new TimerService(),
            Mock.Of<IFocusToastService>(),
            playerMock.Object,
            Mock.Of<IFocusHistoryService>(),
            Mock.Of<ITelemetry>(),
            Mock.Of<IUserSettings>());

        focusService.SetPreviousGlobalVolume(previousVolume);

        // Act
        focusService.StopTimer(sessionCompleted: true, pauseSounds: true);

        //Assert
        Assert.Equal(previousVolume, playerMock.Object.GlobalVolume);
    }
}
