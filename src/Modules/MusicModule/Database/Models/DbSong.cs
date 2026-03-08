using EvoSC.Modules.Official.MusicModule.Interfaces.Models;
using LinqToDB.Mapping;

namespace EvoSC.Modules.Official.MusicModule.Database.Models;

[Table(TableName)]
public class DbSong : ISong
{
    public const string TableName = "Songs";

    [PrimaryKey, Identity]
    public long Id { get; set; }

    [Column]
    public string Url { get; set; } = string.Empty;

    [Column]
    public string Title { get; set; } = string.Empty;

    [Column]
    public string Artist { get; set; } = string.Empty;

    [Column]
    public int DurationSeconds { get; set; }

    [Column]
    public string AddedBy { get; set; } = string.Empty;

    [Column]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
