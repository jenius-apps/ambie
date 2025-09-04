using AmbientSounds.Constants;
using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.Tools;
using JeniusApps.Common.Settings;
using JeniusApps.Common.Tools;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace AmbientSounds.Tests.Services;

public class MixMediaPlayerServiceTests
{
    /// <summary>
    /// Test to confirm that when playing a featured Guide,
    /// the randomly played sound has a non-zero volume.
    /// </summary>
    [Fact]
    public async Task GuideWithRandomSoundHasValidVolumeAsync()
    {
        string testPath = "testPath";
        string testSoundId = "testSoundId";
        var mixMediaPlayerService = new MixMediaPlayerService(
            Mock.Of<ISoundService>(x => x.GetRandomSoundAsync() == Task.FromResult(new Sound { Id = testSoundId, FilePath = testPath })),
            Mock.Of<IAssetLocalizer>(),
            Mock.Of<IDispatcherQueue>(),
            Mock.Of<IMediaPlayerFactory>(x =>
                x.CreatePlayer(It.IsAny<bool>()) == Mock.Of<IMediaPlayer>(x =>
                    x.SetSourceAsync(It.IsAny<string>(), It.IsAny<bool>()) == Task.FromResult(true))),
            Mock.Of<ISystemInfoProvider>(x => x.LocalFolderPath() == testPath),
            Mock.Of<ISystemMediaControls>(),
            Mock.Of<ISoundVolumeService>(),
            Mock.Of<IIapService>(x => x.CanShowPremiumButtonsAsync() == Task.FromResult(true)))
        {
            GlobalVolume = 0.5
        };

        await mixMediaPlayerService.PlayFeaturedSoundAsync(FeaturedSoundType.Guide, "testId", "testPath", addRandomIfNoActives: true);
        Dictionary<string, double> volumes = mixMediaPlayerService.GetPlayerVolumes();

        // Ensure that the random sound is the only sound playing.
        _ = Assert.Single(volumes);
        Assert.Equal(testSoundId, mixMediaPlayerService.GetSoundIds()[0]);
        Assert.True(volumes[testSoundId] > 0); // ensure volume is non-zero
    }
}
