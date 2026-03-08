using EvoSC.Common.Interfaces.Models;
using EvoSC.Common.Services.Attributes;
using EvoSC.Common.Services.Models;
using EvoSC.Modules.Official.MusicModule.Interfaces;
using EvoSC.Modules.Official.MusicModule.Interfaces.Models;
using Microsoft.Extensions.Logging;

namespace EvoSC.Modules.Official.MusicModule.Services;

[Service(LifeStyle = ServiceLifeStyle.Singleton)]
public class PlaylistService(
    ISongRepository songRepository,
    IMusicSettings settings,
    ILogger<PlaylistService> logger) : IPlaylistService
{
    private readonly List<(ISong Song, IPlayer RequestedBy)> _queue = new();
    private readonly object _queueLock = new();
    private long _lastPlayedSongId;

    public IReadOnlyList<(ISong Song, IPlayer RequestedBy)> Queue
    {
        get
        {
            lock (_queueLock)
            {
                return _queue.ToArray();
            }
        }
    }

    public async Task<ISong?> GetNextSongAsync()
    {
        ISong? queuedSong = null;
        lock (_queueLock)
        {
            if (_queue.Count > 0)
            {
                var next = _queue[0];
                _queue.RemoveAt(0);
                _lastPlayedSongId = next.Song.Id;
                logger.LogDebug("Playing queued song: {Title} (requested by {Player})",
                    next.Song.Title, next.RequestedBy.NickName);
                queuedSong = next.Song;
            }
        }

        if (queuedSong != null)
        {
            return queuedSong;
        }

        var songs = (await songRepository.GetAllSongsAsync()).ToList();
        if (songs.Count == 0)
        {
            return null;
        }

        ISong song;
        if (settings.Shuffle)
        {
            var candidates = songs.Where(s => s.Id != _lastPlayedSongId).ToList();
            if (candidates.Count == 0) candidates = songs;
            song = candidates[Random.Shared.Next(candidates.Count)];
        }
        else
        {
            var currentIndex = songs.FindIndex(s => s.Id == _lastPlayedSongId);
            song = songs[(currentIndex + 1) % songs.Count];
        }

        _lastPlayedSongId = song.Id;
        return song;
    }

    public Task<bool> RequestSongAsync(ISong song, IPlayer player)
    {
        lock (_queueLock)
        {
            var playerCount = _queue.Count(q => q.RequestedBy.AccountId == player.AccountId);
            if (playerCount >= settings.MaxRequestsPerPlayer)
            {
                return Task.FromResult(false);
            }

            if (_queue.Any(q => q.Song.Id == song.Id))
            {
                return Task.FromResult(false);
            }

            _queue.Add((song, player));
            logger.LogDebug("Song queued: {Title} by {Player}", song.Title, player.NickName);
            return Task.FromResult(true);
        }
    }

    public void ClearQueue()
    {
        lock (_queueLock)
        {
            _queue.Clear();
        }
    }

    public int GetPlayerRequestCount(IPlayer player)
    {
        lock (_queueLock)
        {
            return _queue.Count(q => q.RequestedBy.AccountId == player.AccountId);
        }
    }
}
