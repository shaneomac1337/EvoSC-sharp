using EvoSC.Common.Interfaces.Models;

namespace EvoSC.Modules.Official.MusicModule.Interfaces;

public interface IVoteSkipService
{
    int VoteCount { get; }
    bool AddVote(IPlayer player);
    bool HasVoted(IPlayer player);
    bool IsThresholdReached(int onlinePlayerCount);
    int VotesNeeded(int onlinePlayerCount);
    void Reset();
}
