using EvoSC.Common.Interfaces;
using EvoSC.Common.Services.Attributes;
using EvoSC.Common.Services.Models;
using EvoSC.Modules.Official.MusicModule.Interfaces;
using EvoSC.Modules.Official.MusicModule.Interfaces.Models;
using GbxRemoteNet;
using Microsoft.Extensions.Logging;

namespace EvoSC.Modules.Official.MusicModule.Services;

[Service(LifeStyle = ServiceLifeStyle.Singleton)]
public class MusicService(
    IServerClient serverClient,
    IPlaylistService playlistService,
    IMusicSettings settings,
    ILogger<MusicService> logger) : IMusicService
{
    public ISong? CurrentSong { get; private set; }

    public async Task PlaySongAsync(ISong song)
    {
        try
        {
            var mc = new MultiCall();
            mc.Add("SetForcedMusic", settings.OverrideMapMusic, song.Url);
            await serverClient.Remote.MultiCallAsync(mc);

            CurrentSong = song;
            logger.LogInformation("Now playing: {Title} by {Artist}", song.Title, song.Artist);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set forced music for song: {Title}", song.Title);
            throw;
        }
    }

    public async Task StopMusicAsync()
    {
        try
        {
            var mc = new MultiCall();
            mc.Add("SetForcedMusic", false, "");
            await serverClient.Remote.MultiCallAsync(mc);

            CurrentSong = null;
            logger.LogInformation("Music stopped");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to stop music");
            throw;
        }
    }

    public async Task AdvanceToNextSongAsync()
    {
        var nextSong = await playlistService.GetNextSongAsync();
        if (nextSong != null)
        {
            await PlaySongAsync(nextSong);
        }
        else
        {
            logger.LogDebug("No songs available to play");
        }
    }
}
