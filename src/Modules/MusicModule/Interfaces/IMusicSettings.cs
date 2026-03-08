using System.ComponentModel;
using Config.Net;
using EvoSC.Modules.Attributes;

namespace EvoSC.Modules.Official.MusicModule.Interfaces;

[Settings]
public interface IMusicSettings
{
    [Option(DefaultValue = true), Description("Override music embedded in maps.")]
    public bool OverrideMapMusic { get; set; }

    [Option(DefaultValue = true), Description("Automatically play the next song on map change.")]
    public bool AutoAdvance { get; set; }

    [Option(DefaultValue = true), Description("Shuffle the song order instead of sequential playback.")]
    public bool Shuffle { get; set; }

    [Option(DefaultValue = 30), Description("Percentage of online players needed to vote-skip a song.")]
    public int SkipThreshold { get; set; }

    [Option(DefaultValue = 1), Description("Maximum number of songs a player can have in the queue at once.")]
    public int MaxRequestsPerPlayer { get; set; }
}
