using EvoSC.Common.Controllers;
using EvoSC.Common.Controllers.Attributes;
using EvoSC.Common.Events.Attributes;
using EvoSC.Common.Interfaces;
using EvoSC.Common.Interfaces.Controllers;
using EvoSC.Common.Remote;
using EvoSC.Modules.Official.MusicModule.Interfaces;
using GbxRemoteNet.Events;
using Microsoft.Extensions.Logging;

namespace EvoSC.Modules.Official.MusicModule.Controllers;

[Controller]
public class MusicEventController(
    IMusicService musicService,
    IVoteSkipService voteSkipService,
    IMusicSettings settings,
    IManialinkManager manialinkManager,
    ILogger<MusicEventController> logger) : EvoScController<IEventControllerContext>
{
    [Subscribe(GbxRemoteEvent.BeginMap)]
    public async Task OnBeginMapAsync(object sender, MapGbxEventArgs args)
    {
        voteSkipService.Reset();

        if (!settings.AutoAdvance)
        {
            return;
        }

        try
        {
            await musicService.AdvanceToNextSongAsync();

            if (musicService.CurrentSong != null)
            {
                await manialinkManager.SendPersistentManialinkAsync("MusicModule.NowPlayingToast",
                    new { title = musicService.CurrentSong.Title, artist = musicService.CurrentSong.Artist });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to advance song on map change");
        }
    }
}
