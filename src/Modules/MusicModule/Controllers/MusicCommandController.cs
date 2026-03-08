using EvoSC.Commands.Attributes;
using EvoSC.Commands.Interfaces;
using EvoSC.Common.Controllers;
using EvoSC.Common.Controllers.Attributes;
using EvoSC.Common.Interfaces;
using EvoSC.Common.Interfaces.Localization;
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
    Locale locale,
    ILogger<MusicCommandController> logger) : EvoScController<ICommandInteractionContext>
{
    private readonly dynamic _locale = locale;

    [ChatCommand("song", "[Command.Song]")]
    public async Task SongAsync()
    {
        var current = musicService.CurrentSong;
        if (current == null)
        {
            await serverClient.Chat.InfoMessageAsync(_locale.PlayerLanguage.NoSongPlaying, Context.Player);
            return;
        }

        await serverClient.Chat.InfoMessageAsync(
            (string)_locale.PlayerLanguage.NowPlaying(current.Title, current.Artist), Context.Player);
    }

    [ChatCommand("music", "[Command.Music]")]
    public async Task MusicBrowserAsync()
    {
        var songs = await songRepository.GetAllSongsAsync();
        var queue = playlistService.Queue;
        await manialinkManager.SendManialinkAsync(Context.Player, "MusicModule.MusicBrowser",
            new { songs, queue, currentSong = musicService.CurrentSong });
    }

    [ChatCommand("request", "[Command.Request]")]
    public async Task RequestAsync(int songId)
    {
        var song = await songRepository.GetSongByIdAsync(songId);
        if (song == null)
        {
            await serverClient.Chat.ErrorMessageAsync(_locale.PlayerLanguage.SongNotFound, Context.Player);
            return;
        }

        var success = await playlistService.RequestSongAsync(song, Context.Player);
        if (!success)
        {
            await serverClient.Chat.ErrorMessageAsync(
                (string)_locale.PlayerLanguage.RequestFailed, Context.Player);
            return;
        }

        await serverClient.Chat.InfoMessageAsync(
            (string)_locale.PlayerLanguage.SongRequested(Context.Player.NickName, song.Title, song.Artist));
    }

    [ChatCommand("skip", "[Command.Skip]")]
    public async Task SkipVoteAsync()
    {
        if (musicService.CurrentSong == null)
        {
            await serverClient.Chat.InfoMessageAsync(_locale.PlayerLanguage.NoSongPlaying, Context.Player);
            return;
        }

        var isNew = voteSkipService.AddVote(Context.Player);
        if (!isNew)
        {
            await serverClient.Chat.InfoMessageAsync(_locale.PlayerLanguage.AlreadyVoted, Context.Player);
            return;
        }

        var onlinePlayers = (await playerManager.GetOnlinePlayersAsync()).Count();
        if (voteSkipService.IsThresholdReached(onlinePlayers))
        {
            await serverClient.Chat.InfoMessageAsync(_locale.PlayerLanguage.VoteSkipPassed);
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
                (string)_locale.PlayerLanguage.VoteSkipProgress(Context.Player.NickName, needed));
        }
    }

    [ChatCommand("addmusic", "[Command.AddMusic]", MusicPermissions.ManageLibrary)]
    [CommandAlias("am", true)]
    public async Task AddMusicAsync(string url, string title, string artist)
    {
        if (!url.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
        {
            await serverClient.Chat.ErrorMessageAsync(
                _locale.PlayerLanguage.OnlyOggSupported, Context.Player);
            return;
        }

        var existing = await songRepository.GetSongByUrlAsync(url);
        if (existing != null)
        {
            await serverClient.Chat.ErrorMessageAsync(_locale.PlayerLanguage.SongUrlExists, Context.Player);
            return;
        }

        var song = await songRepository.AddSongAsync(url, title, artist, 0, Context.Player.AccountId);
        await serverClient.Chat.SuccessMessageAsync(
            (string)_locale.PlayerLanguage.SongAdded(song.Title, song.Artist, song.Id), Context.Player);
    }

    [ChatCommand("removemusic", "[Command.RemoveMusic]", MusicPermissions.ManageLibrary)]
    [CommandAlias("rm", true)]
    public async Task RemoveMusicAsync(int songId)
    {
        var song = await songRepository.GetSongByIdAsync(songId);
        if (song == null)
        {
            await serverClient.Chat.ErrorMessageAsync(_locale.PlayerLanguage.SongNotFound, Context.Player);
            return;
        }

        await songRepository.RemoveSongAsync(songId);
        await serverClient.Chat.SuccessMessageAsync(
            (string)_locale.PlayerLanguage.SongRemoved(song.Title, song.Artist), Context.Player);
    }

    [ChatCommand("forceskip", "[Command.ForceSkip]", MusicPermissions.ForceSkip)]
    public async Task ForceSkipAsync()
    {
        if (musicService.CurrentSong == null)
        {
            await serverClient.Chat.InfoMessageAsync(_locale.PlayerLanguage.NoSongPlaying, Context.Player);
            return;
        }

        await serverClient.Chat.InfoMessageAsync(
            (string)_locale.PlayerLanguage.ForceSkipped(Context.Player.NickName));
        voteSkipService.Reset();
        await musicService.AdvanceToNextSongAsync();

        if (musicService.CurrentSong != null)
        {
            await manialinkManager.SendManialinkAsync("MusicModule.NowPlayingToast",
                new { title = musicService.CurrentSong.Title, artist = musicService.CurrentSong.Artist });
        }
    }

    [ChatCommand("shuffle", "[Command.Shuffle]", MusicPermissions.ManageSettings)]
    public async Task ToggleShuffleAsync()
    {
        settings.Shuffle = !settings.Shuffle;
        var state = settings.Shuffle ? "on" : "off";
        await serverClient.Chat.InfoMessageAsync(
            (string)_locale.PlayerLanguage.ShuffleToggled(state), Context.Player);
    }
}
