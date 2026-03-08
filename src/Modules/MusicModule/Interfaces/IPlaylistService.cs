using EvoSC.Common.Interfaces.Models;
using EvoSC.Modules.Official.MusicModule.Interfaces.Models;

namespace EvoSC.Modules.Official.MusicModule.Interfaces;

public interface IPlaylistService
{
    IReadOnlyList<(ISong Song, IPlayer RequestedBy)> Queue { get; }
    Task<ISong?> GetNextSongAsync();
    Task<bool> RequestSongAsync(ISong song, IPlayer player);
    void ClearQueue();
    int GetPlayerRequestCount(IPlayer player);
}
