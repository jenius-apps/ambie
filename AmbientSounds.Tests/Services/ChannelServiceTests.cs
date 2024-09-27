using AmbientSounds.Models;
using AmbientSounds.Services;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace AmbientSounds.Tests.Services;

public class ChannelServiceTests
{
    [Fact]
    public async Task Queue_NonVideoChannel_Fail()
    {
        var service = new ChannelService(
            Mock.Of<ISoundService>(),
            Mock.Of<IVideoService>(),
            Mock.Of<IIapService>(),
            Mock.Of<IDownloadManager>(),
            Mock.Of<ICatalogueService>());

        var darkScreenChannel = new Channel { Type = ChannelType.DarkScreen };
        var slideshowChannel = new Channel { Type = ChannelType.Slideshow };

        var darkSreenQueued = await service.QueueInstallChannelAsync(darkScreenChannel);
        var slideshowQueued = await service.QueueInstallChannelAsync(slideshowChannel);

        Assert.False(darkSreenQueued);
        Assert.False(slideshowQueued);
    }

    [Fact]
    public async Task Queue_FullyDownloadedChannel_Fail()
    {
        var service = new ChannelService(
            Mock.Of<ISoundService>(x => x.IsSoundInstalledAsync(It.IsAny<string>()) == Task.FromResult(true)),
            Mock.Of<IVideoService>(x => x.IsVideoInstalledAsync(It.IsAny<string>()) == Task.FromResult(true)),
            Mock.Of<IIapService>(),
            Mock.Of<IDownloadManager>(),
            Mock.Of<ICatalogueService>());

        var channel = new Channel
        {
            Type = ChannelType.Videos,
            VideoIds = ["test"],
            SoundIds = ["test"],
        };

        var queued = await service.QueueInstallChannelAsync(channel);

        Assert.False(queued);
    }

    [Fact]
    public async Task Queue_PartialDownloadedChannel_Sound_Pass()
    {
        var downloadManagerMock = new Mock<IDownloadManager>();

        var service = new ChannelService(
            Mock.Of<ISoundService>(x => x.IsSoundInstalledAsync(It.IsAny<string>()) == Task.FromResult(false)),
            Mock.Of<IVideoService>(x => x.IsVideoInstalledAsync(It.IsAny<string>()) == Task.FromResult(true)),
            Mock.Of<IIapService>(),
            downloadManagerMock.Object,
            Mock.Of<ICatalogueService>(x => x.GetSoundsAsync(It.IsAny<IReadOnlyList<string>>()) == Task.FromResult<IReadOnlyList<Sound>>(new Sound[] { new() })));

        var channel = new Channel
        {
            Id = "channelTest",
            Type = ChannelType.Videos,
            VideoIds = ["test"],
            SoundIds = ["test"],
        };

        var queued = await service.QueueInstallChannelAsync(channel);

        downloadManagerMock.Verify(x => x.QueueAndDownloadAsync(It.IsAny<Sound>(), It.IsAny<IProgress<double>>()), Times.Once());
        Assert.True(queued);
    }

    [Fact]
    public async Task Queue_PartialDownloadedChannel_Video_Pass()
    {
        var videoId = "videoTest";
        var videoServiceMock = new Mock<IVideoService>();
        videoServiceMock.Setup(x => x.IsVideoInstalledAsync(videoId)).ReturnsAsync(false);
        videoServiceMock.Setup(x => x.GetVideosAsync(It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync([new Video { Id = videoId }]);

        var service = new ChannelService(
            Mock.Of<ISoundService>(x => x.IsSoundInstalledAsync(It.IsAny<string>()) == Task.FromResult(true)),
            videoServiceMock.Object,
            Mock.Of<IIapService>(),
            Mock.Of<IDownloadManager>(),
            Mock.Of<ICatalogueService>());

        var channel = new Channel
        {
            Id = "channelTest",
            Type = ChannelType.Videos,
            VideoIds = [videoId],
            SoundIds = ["test"],
        };

        var queued = await service.QueueInstallChannelAsync(channel);

        videoServiceMock.Verify(x => x.InstallVideoAsync(It.IsAny<Video>(), It.IsAny<Progress<double>>()), Times.Once());
        Assert.True(queued);
    }

    [Fact]
    public async Task Queue_Channel_AllAssets_Pass()
    {
        var videoId = "videoTest";
        var soundId = "soundTest";
        var downloadManagerMock = new Mock<IDownloadManager>();
        var videoServiceMock = new Mock<IVideoService>();
        videoServiceMock.Setup(x => x.IsVideoInstalledAsync(videoId)).ReturnsAsync(false);
        videoServiceMock.Setup(x => x.GetVideosAsync(It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync([new() { Id = videoId }]);

        var service = new ChannelService(
            Mock.Of<ISoundService>(x => x.IsSoundInstalledAsync(It.IsAny<string>()) == Task.FromResult(false)),
            videoServiceMock.Object,
            Mock.Of<IIapService>(),
            downloadManagerMock.Object,
            Mock.Of<ICatalogueService>(x => x.GetSoundsAsync(It.IsAny<IReadOnlyList<string>>()) == Task.FromResult<IReadOnlyList<Sound>>(new Sound[] { new() { Id = soundId } })));

        var channel = new Channel
        {
            Id = "channelTest",
            Type = ChannelType.Videos,
            VideoIds = [videoId],
            SoundIds = [soundId],
        };

        var queued = await service.QueueInstallChannelAsync(channel);

        downloadManagerMock.Verify(x => x.QueueAndDownloadAsync(It.IsAny<Sound>(), It.IsAny<IProgress<double>>()), Times.Once());
        videoServiceMock.Verify(x => x.InstallVideoAsync(It.IsAny<Video>(), It.IsAny<Progress<double>>()), Times.Once());
        Assert.True(queued);
    }
}
