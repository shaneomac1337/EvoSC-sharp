using EvoSC.Common.Controllers;
using EvoSC.Common.Controllers.Attributes;
using EvoSC.Common.Interfaces;
using EvoSC.Common.Interfaces.Controllers;
using EvoSC.Manialinks.Attributes;
using EvoSC.Modules.Official.MusicModule.Interfaces;

namespace EvoSC.Modules.Official.MusicModule.Controllers;

[Controller]
public class MusicManialinkController(
    IPlaylistService playlistService,
    ISongRepository songRepository,
    IManialinkManager manialinkManager,
    IMusicService musicService,
    IServerClient serverClient) : EvoScController<IManialinkInteractionContext>
{
    [ManialinkRoute]
    public async Task RequestFromBrowserAsync(long songId)
    {
        var song = await songRepository.GetSongByIdAsync(songId);
        if (song == null)
        {
            return;
        }

        var success = await playlistService.RequestSongAsync(song, Context.Player);
        if (success)
        {
            await serverClient.Chat.InfoMessageAsync(
                $"$<$fff{Context.Player.NickName}$> requested: $<$fff{song.Title}$> by $<$fff{song.Artist}$>");
        }
        else
        {
            await serverClient.Chat.ErrorMessageAsync(
                "Cannot request this song. Limit reached or already queued.", Context.Player);
        }

        // Refresh the browser
        var songs = await songRepository.GetAllSongsAsync();
        var queue = playlistService.Queue;
        await manialinkManager.SendManialinkAsync(Context.Player, "MusicModule.MusicBrowser",
            new { songs, queue, currentSong = musicService.CurrentSong });
    }
}
