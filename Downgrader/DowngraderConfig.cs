using GbxToolAPI;

namespace Downgrader;

public class DowngraderConfig : Config {
    //RemoveMediatracker : Removes mediatracker from the downgraded map completely. Default is false.
    public bool RemoveMediatracker { get; set; } = false;
}