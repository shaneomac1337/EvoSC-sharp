namespace EvoSC.Modules.Official.MusicModule.Interfaces.Models;

public interface ISong
{
    public long Id { get; }
    public string Url { get; }
    public string Title { get; }
    public string Artist { get; }
    public int DurationSeconds { get; }
    public string AddedBy { get; }
    public DateTime CreatedAt { get; }
}
