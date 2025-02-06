using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using JeniusApps.Common.Telemetry;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace AmbientSounds.Tests.ViewModels;

public class ChannelViewModelTests
{
    [Theory]
    [InlineData(true, true, true)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(false, false, false)]
    public async void Initialize_PlayButtonVisibility(bool isFullyDownloaded, bool isOwned, bool expectedResult)
    {
        var vm = GetViewModel(isFullyDownloaded, isOwned);
        await vm.InitializeAsync();
        Assert.Equal(expectedResult, vm.PlayButtonVisible);
    }

    [Theory]
    [InlineData(true, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, false, false)]
    public async void Initialize_DownloadButtonVisibility(bool isFullyDownloaded, bool isOwned, bool expectedResult)
    {
        var vm = GetViewModel(isFullyDownloaded, isOwned);
        await vm.InitializeAsync();
        Assert.Equal(expectedResult, vm.DownloadButtonVisible);
    }

    [Theory]
    [InlineData(true, true, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, true)]
    [InlineData(false, false, true)]
    public async void Initialize_BuyButtonVisibility(bool isFullyDownloaded, bool isOwned, bool expectedResult)
    {
        var vm = GetViewModel(isFullyDownloaded, isOwned);
        await vm.InitializeAsync();
        Assert.Equal(expectedResult, vm.BuyButtonVisible);
    }

    private static ChannelViewModel GetViewModel(bool isFullyDownloaded, bool isOwned)
    {
        var channelServiceMock = new Mock<IChannelService>();
        channelServiceMock.Setup(service => service.IsFullyDownloadedAsync(It.IsAny<Channel>())).Returns(Task.FromResult(isFullyDownloaded));
        channelServiceMock.Setup(service => service.IsOwnedAsync(It.IsAny<Channel>())).Returns(Task.FromResult(isOwned));

        var vm = new ChannelViewModel(
            It.IsAny<Channel>(),
            Mock.Of<IAssetLocalizer>(),
            channelServiceMock.Object,
            Mock.Of<IDialogService>(),
            Mock.Of<IIapService>(),
            Mock.Of<ITelemetry>());

        return vm;
    }
}
