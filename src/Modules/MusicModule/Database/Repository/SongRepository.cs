using EvoSC.Common.Database.Repository;
using EvoSC.Common.Interfaces.Database;
using EvoSC.Common.Services.Attributes;
using EvoSC.Common.Services.Models;
using EvoSC.Modules.Official.MusicModule.Database.Models;
using EvoSC.Modules.Official.MusicModule.Interfaces;
using LinqToDB;

namespace EvoSC.Modules.Official.MusicModule.Database.Repository;

[Service(LifeStyle = ServiceLifeStyle.Transient)]
public class SongRepository(IDbConnectionFactory dbConnFactory)
    : DbRepository(dbConnFactory), ISongRepository
{
    public async Task<IEnumerable<DbSong>> GetAllSongsAsync() =>
        await Table<DbSong>()
            .OrderBy(s => s.Title)
            .ToArrayAsync();

    public async Task<DbSong?> GetSongByIdAsync(long id) =>
        await Table<DbSong>()
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<DbSong?> GetSongByUrlAsync(string url) =>
        await Table<DbSong>()
            .FirstOrDefaultAsync(s => s.Url == url);

    public async Task<DbSong> AddSongAsync(string url, string title, string artist, int durationSeconds, string addedBy)
    {
        var song = new DbSong
        {
            Url = url,
            Title = title,
            Artist = artist,
            DurationSeconds = durationSeconds,
            AddedBy = addedBy,
            CreatedAt = DateTime.UtcNow
        };

        var id = await Database.InsertWithIdentityAsync(song);
        song.Id = Convert.ToInt64(id);
        return song;
    }

    public async Task RemoveSongAsync(long id) =>
        await Table<DbSong>()
            .Where(s => s.Id == id)
            .DeleteAsync();

    public async Task<int> GetSongCountAsync() =>
        await Table<DbSong>().CountAsync();
}
