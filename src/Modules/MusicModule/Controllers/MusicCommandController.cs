using EvoSC.Commands.Attributes;
using EvoSC.Commands.Interfaces;
using EvoSC.Common.Controllers;
using EvoSC.Common.Controllers.Attributes;
using EvoSC.Common.Interfaces;
using EvoSC.Common.Interfaces.Services;
using EvoSC.Manialinks.Interfaces;
using EvoSC.Modules.Official.MusicModule.Interfaces;
using Microsoft.Extensions.Logging;

namespace EvoSC.Modules.Official.MusicModule.Controllers;

[Controller]
public class MusicCommandController(
    IMusicService musicService,
    IPlaylistService playlistService,
    IVoteSkipService voteSkipService,
    ISongRepository songRepository,
    IManialinkManager manialinkManager,
    IPlayerManagerService playerManager,
    IMusicSettings settings,
    IServerClient serverClient,
    ILogger<MusicCommandController> logger) : EvoScController<ICommandInteractionContext>
{
    [ChatCommand("song", "Show the currently playing song.")]
    public async Task SongAsync()
    {
        var current = musicService.CurrentSong;
        if (current == null)
        {
            await serverClient.Chat.InfoMessageAsync("No song is currently playing.", Context.Player);
            return;
        }

        await serverClient.Chat.InfoMessageAsync(
            $"Now playing: $<$fff{current.Title}$> by $<$fff{current.Artist}$>", Context.Player);
    }

    [ChatCommand("music", "Open the music browser.")]
    public async Task MusicBrowserAsync()
    {
        var songs = await songRepository.GetAllSongsAsync();
        var queue = playlistService.Queue;
        await manialinkManager.SendManialinkAsync(Context.Player, "MusicModule.MusicBrowser",
            new { songs, queue, currentSong = musicService.CurrentSong });
    }

    [ChatCommand("request", "Request a song by ID.")]
    public async Task RequestAsync(int songId)
    {
        var song = await songRepository.GetSongByIdAsync(songId);
        if (song == null)
        {
            await serverClient.Chat.ErrorMessageAsync("Song not found.", Context.Player);
            return;
        }

        var success = await playlistService.RequestSongAsync(song, Context.Player);
        if (!success)
        {
            await serverClient.Chat.ErrorMessageAsync(
                "Cannot request this song. You may have reached your request limit or this song is already queued.",
                Context.Player);
            return;
        }

        await serverClient.Chat.InfoMessageAsync(
            $"$<$fff{Context.Player.NickName}$> requested: $<$fff{song.Title}$> by $<$fff{song.Artist}$>");
    }

    [ChatCommand("skip", "Vote to skip the current song.")]
    public async Task SkipVoteAsync()
    {
        if (musicService.CurrentSong == null)
        {
            await serverClient.Chat.InfoMessageAsync("No song is currently playing.", Context.Player);
            return;
        }

        var isNew = voteSkipService.AddVote(Context.Player);
        if (!isNew)
        {
            await serverClient.Chat.InfoMessageAsync("You have already voted to skip.", Context.Player);
            return;
        }

        var onlinePlayers = (await playerManager.GetOnlinePlayersAsync()).Count();
        if (voteSkipService.IsThresholdReached(onlinePlayers))
        {
            await serverClient.Chat.InfoMessageAsync("Vote skip passed! Skipping song...");
            voteSkipService.Reset();
            await musicService.AdvanceToNextSongAsync();

            if (musicService.CurrentSong != null)
            {
                await manialinkManager.SendManialinkAsync("MusicModule.NowPlayingToast",
                    new { title = musicService.CurrentSong.Title, artist = musicService.CurrentSong.Artist });
            }
        }
        else
        {
            var needed = voteSkipService.VotesNeeded(onlinePlayers);
            await serverClient.Chat.InfoMessageAsync(
                $"$<$fff{Context.Player.NickName}$> voted to skip. {needed} more vote(s) needed.");
        }
    }

    [ChatCommand("addmusic", "Add a song to the library.", MusicPermissions.ManageLibrary)]
    [CommandAlias("am", true)]
    public async Task AddMusicAsync(string url, string title, string artist)
    {
        if (!url.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
        {
            await serverClient.Chat.ErrorMessageAsync(
                "Only .ogg files are supported by Trackmania.", Context.Player);
            return;
        }

        var existing = await songRepository.GetSongByUrlAsync(url);
        if (existing != null)
        {
            await serverClient.Chat.ErrorMessageAsync("This song URL is already in the library.", Context.Player);
            return;
        }

        var song = await songRepository.AddSongAsync(url, title, artist, 0, Context.Player.AccountId);
        await serverClient.Chat.SuccessMessageAsync(
            $"Added: $<$fff{song.Title}$> by $<$fff{song.Artist}$> (ID: {song.Id})", Context.Player);
    }

    [ChatCommand("removemusic", "Remove a song from the library.", MusicPermissions.ManageLibrary)]
    [CommandAlias("rm", true)]
    public async Task RemoveMusicAsync(int songId)
    {
        var song = await songRepository.GetSongByIdAsync(songId);
        if (song == null)
        {
            await serverClient.Chat.ErrorMessageAsync("Song not found.", Context.Player);
            return;
        }

        await songRepository.RemoveSongAsync(songId);
        await serverClient.Chat.SuccessMessageAsync(
            $"Removed: $<$fff{song.Title}$> by $<$fff{song.Artist}$>", Context.Player);
    }

    [ChatCommand("forceskip", "Force skip the current song.", MusicPermissions.ForceSkip)]
    public async Task ForceSkipAsync()
    {
        if (musicService.CurrentSong == null)
        {
            await serverClient.Chat.InfoMessageAsync("No song is currently playing.", Context.Player);
            return;
        }

        await serverClient.Chat.InfoMessageAsync(
            $"$<$fff{Context.Player.NickName}$> force-skipped the current song.");
        voteSkipService.Reset();
        await musicService.AdvanceToNextSongAsync();

        if (musicService.CurrentSong != null)
        {
            await manialinkManager.SendManialinkAsync("MusicModule.NowPlayingToast",
                new { title = musicService.CurrentSong.Title, artist = musicService.CurrentSong.Artist });
        }
    }

    [ChatCommand("shuffle", "Toggle shuffle mode.", MusicPermissions.ManageSettings)]
    public async Task ToggleShuffleAsync()
    {
        settings.Shuffle = !settings.Shuffle;
        var state = settings.Shuffle ? "on" : "off";
        await serverClient.Chat.InfoMessageAsync($"Shuffle is now $<$fff{state}$>.", Context.Player);
    }
}
