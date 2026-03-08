using System.ComponentModel;
using EvoSC.Common.Permissions.Attributes;

namespace EvoSC.Modules.Official.MusicModule;

[PermissionGroup]
public enum MusicPermissions
{
    [Description("Add or remove songs from the music library.")]
    ManageLibrary,

    [Description("Force skip the current song.")]
    ForceSkip,

    [Description("Change music module settings.")]
    ManageSettings
}
