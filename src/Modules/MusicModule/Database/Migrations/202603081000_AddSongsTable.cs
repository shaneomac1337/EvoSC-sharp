using EvoSC.Modules.Official.MusicModule.Database.Models;
using FluentMigrator;

namespace EvoSC.Modules.Official.MusicModule.Database.Migrations;

[Tags("Production")]
[Migration(1741428000)]
public class AddSongsTable : Migration
{
    public override void Up()
    {
        Create.Table(DbSong.TableName)
            .WithColumn(nameof(DbSong.Id)).AsInt64().PrimaryKey().Identity()
            .WithColumn(nameof(DbSong.Url)).AsString(512).NotNullable()
            .WithColumn(nameof(DbSong.Title)).AsString(256).NotNullable()
            .WithColumn(nameof(DbSong.Artist)).AsString(256).NotNullable()
            .WithColumn(nameof(DbSong.DurationSeconds)).AsInt32().WithDefaultValue(0)
            .WithColumn(nameof(DbSong.AddedBy)).AsString(256).NotNullable()
            .WithColumn(nameof(DbSong.CreatedAt)).AsDateTime().NotNullable();
    }

    public override void Down()
    {
        Delete.Table(DbSong.TableName);
    }
}
