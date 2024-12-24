using AmbientSounds.Cache;
using AmbientSounds.Events;
using AmbientSounds.Models;
using AmbientSounds.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace AmbientSounds.Tests.Services;

public class ChannelServiceTests
{
    [Fact]
    public async Task Play_VideoChannel_PlayerRandom_Success()
    {
        var playerMock = new Mock<IMixMediaPlayerService>();
        playerMock.Setup(x => x.GetSoundIds()).Returns([]);

        var service = new ChannelService(
            Mock.Of<IChannelCache>(),
            Mock.Of<ISoundService>(),
            Mock.Of<IVideoService>(),
            Mock.Of<IIapService>(),
            Mock.Of<IDownloadManager>(),
            Mock.Of<ICatalogueService>(),
            Mock.Of<INavigator>(),
            playerMock.Object);

        var videoChannel = new Channel { Type = ChannelType.Videos, VideoIds = ["test"], SoundIds = [] };
        await service.PlayChannelAsync(videoChannel);
        playerMock.Verify(x => x.AddRandomAsync(), Times.Once());
    }

    [Fact]
    public async Task Play_NonVideoChannel_PlayerRandom_Success()
    {
        var playerMock = new Mock<IMixMediaPlayerService>();
        playerMock.Setup(x => x.GetSoundIds()).Returns([]);

        var service = new ChannelService(
            Mock.Of<IChannelCache>(),
            Mock.Of<ISoundService>(),
            Mock.Of<IVideoService>(),
            Mock.Of<IIapService>(),
            Mock.Of<IDownloadManager>(),
            Mock.Of<ICatalogueService>(),
            Mock.Of<INavigator>(),
            playerMock.Object);

        var darkScreenChannel = new Channel { Type = ChannelType.DarkScreen };
        await service.PlayChannelAsync(darkScreenChannel);
        playerMock.Verify(x => x.AddRandomAsync(), Times.Once());

        playerMock.Reset();

        var slideshowChannel = new Channel { Type = ChannelType.Slideshow };
        await service.PlayChannelAsync(slideshowChannel);
        playerMock.Verify(x => x.AddRandomAsync(), Times.Once());
    }

    [Fact]
    public async Task Play_NonVideoChannel_PlayerActive_Success()
    {
        var playerMock = new Mock<IMixMediaPlayerService>();
        playerMock.Setup(x => x.GetSoundIds()).Returns(["test", "test"]);

        var service = new ChannelService(
            Mock.Of<IChannelCache>(),
            Mock.Of<ISoundService>(),
            Mock.Of<IVideoService>(),
            Mock.Of<IIapService>(),
            Mock.Of<IDownloadManager>(),
            Mock.Of<ICatalogueService>(),
            Mock.Of<INavigator>(),
            playerMock.Object);

        var darkScreenChannel = new Channel { Type = ChannelType.DarkScreen };
        await service.PlayChannelAsync(darkScreenChannel);
        playerMock.Verify(x => x.Play(), Times.Once());

        playerMock.Reset();
        playerMock.Setup(x => x.GetSoundIds()).Returns(["test", "test"]);

        var slideshowChannel = new Channel { Type = ChannelType.Slideshow };
        await service.PlayChannelAsync(slideshowChannel);
        playerMock.Verify(x => x.Play(), Times.Once());
    }

    [Fact]
    public async Task Play_NonVideoChannel_Navigate_Success()
    {
        var navigatorMock = new Mock<INavigator>();

        var service = new ChannelService(
            Mock.Of<IChannelCache>(),
            Mock.Of<ISoundService>(),
            Mock.Of<IVideoService>(),
            Mock.Of<IIapService>(),
            Mock.Of<IDownloadManager>(),
            Mock.Of<ICatalogueService>(),
            navigatorMock.Object,
            Mock.Of<IMixMediaPlayerService>());

        var darkScreenChannel = new Channel { Type = ChannelType.DarkScreen };
        var slideshowChannel = new Channel { Type = ChannelType.Slideshow };

        await service.PlayChannelAsync(darkScreenChannel);
        navigatorMock.Verify(x => x.ToScreensaver(It.IsAny<ScreensaverArgs>()), Times.Once());

        navigatorMock.Reset();

        await service.PlayChannelAsync(slideshowChannel);
        navigatorMock.Verify(x => x.ToScreensaver(It.IsAny<ScreensaverArgs>()), Times.Once());
    }

    [Fact]
    public async Task Queue_NonVideoChannel_Fail()
    {
        var service = new ChannelService(
            Mock.Of<IChannelCache>(),
            Mock.Of<ISoundService>(),
            Mock.Of<IVideoService>(),
            Mock.Of<IIapService>(),
            Mock.Of<IDownloadManager>(),
            Mock.Of<ICatalogueService>(),
            Mock.Of<INavigator>(),
            Mock.Of<IMixMediaPlayerService>());

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
            Mock.Of<IChannelCache>(),
            Mock.Of<ISoundService>(x => x.IsSoundInstalledAsync(It.IsAny<string>()) == Task.FromResult(true)),
            Mock.Of<IVideoService>(x => x.IsVideoInstalledAsync(It.IsAny<string>()) == Task.FromResult(true)),
            Mock.Of<IIapService>(),
            Mock.Of<IDownloadManager>(),
            Mock.Of<ICatalogueService>(),
            Mock.Of<INavigator>(),
            Mock.Of<IMixMediaPlayerService>());

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
            Mock.Of<IChannelCache>(),
            Mock.Of<ISoundService>(x => x.IsSoundInstalledAsync(It.IsAny<string>()) == Task.FromResult(false)),
            Mock.Of<IVideoService>(x => x.IsVideoInstalledAsync(It.IsAny<string>()) == Task.FromResult(true)),
            Mock.Of<IIapService>(),
            downloadManagerMock.Object,
            Mock.Of<ICatalogueService>(x => x.GetSoundsAsync(It.IsAny<IReadOnlyList<string>>()) == Task.FromResult<IReadOnlyList<Sound>>(new Sound[] { new() })),
            Mock.Of<INavigator>(),
            Mock.Of<IMixMediaPlayerService>());

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

        var duplicateQueue = await service.QueueInstallChannelAsync(channel);
        Assert.False(duplicateQueue);
    }

    [Fact]
    public async Task Queue_PartialDownloadedChannel_Video_Pass()
    {
        var videoId = "videoTest";
        var videoServiceMock = new Mock<IVideoService>();
        videoServiceMock.Setup(x => x.IsVideoInstalledAsync(videoId)).ReturnsAsync(false);
        videoServiceMock.Setup(x => x.GetVideosAsync(It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync([new Video { Id = videoId }]);

        var service = new ChannelService(
            Mock.Of<IChannelCache>(),
            Mock.Of<ISoundService>(x => x.IsSoundInstalledAsync(It.IsAny<string>()) == Task.FromResult(true)),
            videoServiceMock.Object,
            Mock.Of<IIapService>(),
            Mock.Of<IDownloadManager>(),
            Mock.Of<ICatalogueService>(),
            Mock.Of<INavigator>(),
            Mock.Of<IMixMediaPlayerService>());

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

        var duplicateQueue = await service.QueueInstallChannelAsync(channel);
        Assert.False(duplicateQueue);
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
            Mock.Of<IChannelCache>(),
            Mock.Of<ISoundService>(x => x.IsSoundInstalledAsync(It.IsAny<string>()) == Task.FromResult(false)),
            videoServiceMock.Object,
            Mock.Of<IIapService>(),
            downloadManagerMock.Object,
            Mock.Of<ICatalogueService>(x => x.GetSoundsAsync(It.IsAny<IReadOnlyList<string>>()) == Task.FromResult<IReadOnlyList<Sound>>(new Sound[] { new() { Id = soundId } })),
            Mock.Of<INavigator>(),
            Mock.Of<IMixMediaPlayerService>());

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

    [Fact]
    public async Task Queue_Channel_VideoAssetOnly_Pass()
    {
        var videoId = "videoTest";
        var videoServiceMock = new Mock<IVideoService>();
        videoServiceMock.Setup(x => x.IsVideoInstalledAsync(videoId)).ReturnsAsync(false);
        videoServiceMock.Setup(x => x.GetVideosAsync(It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync([new() { Id = videoId }]);

        var service = new ChannelService(
            Mock.Of<IChannelCache>(),
            Mock.Of<ISoundService>(),
            videoServiceMock.Object,
            Mock.Of<IIapService>(),
            Mock.Of<IDownloadManager>(),
            Mock.Of<ICatalogueService>(),
            Mock.Of<INavigator>(),
            Mock.Of<IMixMediaPlayerService>());

        var channel = new Channel
        {
            Id = "channelTest",
            Type = ChannelType.Videos,
            VideoIds = [videoId],
            SoundIds = [],
        };

        var queued = await service.QueueInstallChannelAsync(channel);

        videoServiceMock.Verify(x => x.InstallVideoAsync(It.IsAny<Video>(), It.IsAny<Progress<double>>()), Times.Once());
        Assert.True(queued);
    }
}
