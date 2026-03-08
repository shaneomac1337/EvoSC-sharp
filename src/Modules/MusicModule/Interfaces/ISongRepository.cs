using EvoSC.Modules.Official.MusicModule.Database.Models;

namespace EvoSC.Modules.Official.MusicModule.Interfaces;

public interface ISongRepository
{
    Task<IEnumerable<DbSong>> GetAllSongsAsync();
    Task<DbSong?> GetSongByIdAsync(long id);
    Task<DbSong?> GetSongByUrlAsync(string url);
    Task<DbSong> AddSongAsync(string url, string title, string artist, int durationSeconds, string addedBy);
    Task RemoveSongAsync(long id);
    Task<int> GetSongCountAsync();
}
