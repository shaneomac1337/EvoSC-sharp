using EvoSC.Common.Interfaces.Models;
using EvoSC.Modules.Official.MusicModule.Database.Models;
using EvoSC.Modules.Official.MusicModule.Interfaces;
using EvoSC.Modules.Official.MusicModule.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MusicModule.Tests.Services;

public class PlaylistServiceTests
{
    private static (PlaylistService Service, Mock<ISongRepository> Repo) CreateService(
        int maxRequests = 1, bool shuffle = true)
    {
        var repo = new Mock<ISongRepository>();
        var settings = new Mock<IMusicSettings>();
        settings.Setup(s => s.MaxRequestsPerPlayer).Returns(maxRequests);
        settings.Setup(s => s.Shuffle).Returns(shuffle);
        var logger = new Mock<ILogger<PlaylistService>>();
        var service = new PlaylistService(repo.Object, settings.Object, logger.Object);
        return (service, repo);
    }

    private static IPlayer MockPlayer(string accountId, string nickName = "Player")
    {
        var player = new Mock<IPlayer>();
        player.Setup(p => p.AccountId).Returns(accountId);
        player.Setup(p => p.NickName).Returns(nickName);
        return player.Object;
    }

    private static DbSong MockSong(long id, string title = "Test Song") =>
        new() { Id = id, Title = title, Artist = "Test Artist", Url = $"http://example.com/{id}.ogg" };

    [Fact]
    public async Task RequestSong_Success()
    {
        var (service, _) = CreateService();
        var player = MockPlayer("p1");
        var song = MockSong(1);

        var result = await service.RequestSongAsync(song, player);

        Assert.True(result);
        Assert.Single(service.Queue);
    }

    [Fact]
    public async Task RequestSong_ExceedsLimit_ReturnsFalse()
    {
        var (service, _) = CreateService(maxRequests: 1);
        var player = MockPlayer("p1");

        await service.RequestSongAsync(MockSong(1), player);
        var result = await service.RequestSongAsync(MockSong(2), player);

        Assert.False(result);
        Assert.Single(service.Queue);
    }

    [Fact]
    public async Task RequestSong_DuplicateSong_ReturnsFalse()
    {
        var (service, _) = CreateService(maxRequests: 5);
        var song = MockSong(1);

        await service.RequestSongAsync(song, MockPlayer("p1"));
        var result = await service.RequestSongAsync(song, MockPlayer("p2"));

        Assert.False(result);
    }

    [Fact]
    public async Task GetNextSong_QueuedFirst()
    {
        var (service, _) = CreateService();
        var song = MockSong(1, "Queued Song");
        await service.RequestSongAsync(song, MockPlayer("p1"));

        var next = await service.GetNextSongAsync();

        Assert.NotNull(next);
        Assert.Equal("Queued Song", next!.Title);
        Assert.Empty(service.Queue);
    }

    [Fact]
    public async Task GetNextSong_FallsBackToLibrary()
    {
        var (service, repo) = CreateService(shuffle: false);
        var songs = new[] { MockSong(1, "Song A"), MockSong(2, "Song B") };
        repo.Setup(r => r.GetAllSongsAsync()).ReturnsAsync(songs);

        var next = await service.GetNextSongAsync();

        Assert.NotNull(next);
    }

    [Fact]
    public async Task GetNextSong_EmptyLibrary_ReturnsNull()
    {
        var (service, repo) = CreateService();
        repo.Setup(r => r.GetAllSongsAsync()).ReturnsAsync(Array.Empty<DbSong>());

        var next = await service.GetNextSongAsync();

        Assert.Null(next);
    }

    [Fact]
    public void ClearQueue_RemovesAllEntries()
    {
        var (service, _) = CreateService(maxRequests: 5);
        service.RequestSongAsync(MockSong(1), MockPlayer("p1")).Wait();
        service.RequestSongAsync(MockSong(2), MockPlayer("p2")).Wait();

        service.ClearQueue();

        Assert.Empty(service.Queue);
    }

    [Fact]
    public void GetPlayerRequestCount_ReturnsCorrectCount()
    {
        var (service, _) = CreateService(maxRequests: 5);
        var player = MockPlayer("p1");
        service.RequestSongAsync(MockSong(1), player).Wait();
        service.RequestSongAsync(MockSong(2), player).Wait();

        Assert.Equal(2, service.GetPlayerRequestCount(player));
    }
}
