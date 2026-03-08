using EvoSC.Common.Interfaces.Models;
using EvoSC.Modules.Official.MusicModule.Interfaces;
using EvoSC.Modules.Official.MusicModule.Services;
using Moq;
using Xunit;

namespace MusicModule.Tests.Services;

public class VoteSkipServiceTests
{
    private static IVoteSkipService CreateService(int skipThreshold = 30)
    {
        var settings = new Mock<IMusicSettings>();
        settings.Setup(s => s.SkipThreshold).Returns(skipThreshold);
        return new VoteSkipService(settings.Object);
    }

    private static IPlayer MockPlayer(string accountId)
    {
        var player = new Mock<IPlayer>();
        player.Setup(p => p.AccountId).Returns(accountId);
        return player.Object;
    }

    [Fact]
    public void AddVote_NewPlayer_ReturnsTrue()
    {
        var service = CreateService();
        var player = MockPlayer("player1");

        var result = service.AddVote(player);

        Assert.True(result);
        Assert.Equal(1, service.VoteCount);
    }

    [Fact]
    public void AddVote_SamePlayer_ReturnsFalse()
    {
        var service = CreateService();
        var player = MockPlayer("player1");

        service.AddVote(player);
        var result = service.AddVote(player);

        Assert.False(result);
        Assert.Equal(1, service.VoteCount);
    }

    [Fact]
    public void IsThresholdReached_BelowThreshold_ReturnsFalse()
    {
        var service = CreateService(50);
        service.AddVote(MockPlayer("player1"));

        Assert.False(service.IsThresholdReached(10));
    }

    [Fact]
    public void IsThresholdReached_AtThreshold_ReturnsTrue()
    {
        var service = CreateService(50);
        service.AddVote(MockPlayer("player1"));
        service.AddVote(MockPlayer("player2"));
        service.AddVote(MockPlayer("player3"));

        Assert.True(service.IsThresholdReached(5));
    }

    [Fact]
    public void Reset_ClearsAllVotes()
    {
        var service = CreateService();
        service.AddVote(MockPlayer("player1"));
        service.AddVote(MockPlayer("player2"));

        service.Reset();

        Assert.Equal(0, service.VoteCount);
    }

    [Fact]
    public void VotesNeeded_CalculatesCorrectly()
    {
        var service = CreateService(30);
        service.AddVote(MockPlayer("player1"));

        Assert.Equal(2, service.VotesNeeded(10));
    }

    [Fact]
    public void HasVoted_ReturnsTrueAfterVoting()
    {
        var service = CreateService();
        var player = MockPlayer("player1");

        service.AddVote(player);

        Assert.True(service.HasVoted(player));
    }

    [Fact]
    public void HasVoted_ReturnsFalseBeforeVoting()
    {
        var service = CreateService();
        var player = MockPlayer("player1");

        Assert.False(service.HasVoted(player));
    }
}
