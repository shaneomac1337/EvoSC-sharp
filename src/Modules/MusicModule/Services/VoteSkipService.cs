using EvoSC.Common.Interfaces.Models;
using EvoSC.Common.Services.Attributes;
using EvoSC.Common.Services.Models;
using EvoSC.Modules.Official.MusicModule.Interfaces;

namespace EvoSC.Modules.Official.MusicModule.Services;

[Service(LifeStyle = ServiceLifeStyle.Singleton)]
public class VoteSkipService(IMusicSettings settings) : IVoteSkipService
{
    private readonly HashSet<string> _votes = new();
    private readonly object _voteLock = new();

    public int VoteCount
    {
        get
        {
            lock (_voteLock)
            {
                return _votes.Count;
            }
        }
    }

    public bool AddVote(IPlayer player)
    {
        lock (_voteLock)
        {
            return _votes.Add(player.AccountId);
        }
    }

    public bool HasVoted(IPlayer player)
    {
        lock (_voteLock)
        {
            return _votes.Contains(player.AccountId);
        }
    }

    public bool IsThresholdReached(int onlinePlayerCount)
    {
        if (onlinePlayerCount <= 0) return false;

        lock (_voteLock)
        {
            var threshold = Math.Max(1, (int)Math.Ceiling(onlinePlayerCount * settings.SkipThreshold / 100.0));
            return _votes.Count >= threshold;
        }
    }

    public int VotesNeeded(int onlinePlayerCount)
    {
        if (onlinePlayerCount <= 0) return 1;

        lock (_voteLock)
        {
            var threshold = Math.Max(1, (int)Math.Ceiling(onlinePlayerCount * settings.SkipThreshold / 100.0));
            return Math.Max(0, threshold - _votes.Count);
        }
    }

    public void Reset()
    {
        lock (_voteLock)
        {
            _votes.Clear();
        }
    }
}
