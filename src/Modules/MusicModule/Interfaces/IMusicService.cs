using EvoSC.Modules.Official.MusicModule.Interfaces.Models;

namespace EvoSC.Modules.Official.MusicModule.Interfaces;

public interface IMusicService
{
    ISong? CurrentSong { get; }
    Task PlaySongAsync(ISong song);
    Task StopMusicAsync();
    Task AdvanceToNextSongAsync();
}
