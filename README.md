# Downgrader [(online)](https://gbx.bigbang1112.cz/tool/downgrader)

Hosted on [Gbx Web Tools](https://github.com/bigbang1112-cz/gbx), lives on [Gbx Tool API](https://github.com/bigbang1112-cz/gbx-tool-api), internally powered by [GBX.NET](https://github.com/BigBang1112/gbx-net).

**Downgrader** is a tool that can convert TrackMania² Stadium maps to TrackMania Forever. It is the successor to the now archived [UnMap](https://github.com/LinUwUxCat/UnMap).

Features of Downgrader:
- Blocks are adjusted to be kept at the correct position
- Mood is saved
- Mediatracker is mostly supported **(new!)**
  - All mediatracker blocks except ghosts are supported
- Ability to completely remove mediatracker if you want to

[IMAGES]

## CLI build

For 100% offline control, you can use the CLI version of Downgrader. Drag and drop your desired maps onto the DowngraderCLI(.exe).

### ConsoleOptions.yml

- **NoPause** - If true, All "Press key to continue..." will be skipped.
- **SingleOutput** - If false, dragging multiple files will produce multiple results. If true, multiple files will produce only one result.
- **CustomConfig** - Name of the config inside the `Config` folder without the extension.
- **OutputDir** - Forced output directory of produced results.

Location where the game exe is (you will be asked for it if ConsoleOptions.yml does not exist):

- **TrackmaniaForeverInstallationPath**
- **ManiaPlanetInstallationPath**
- **TrackmaniaTurboInstallationPath** 
- **Trackmania2020InstallationPath**

### Update notifications

The tool notifies you about new versions after launching it. You can press U to directly open the web page where you can download the new version. For security reasons, auto-updater is not planned atm.

### Specific command line arguments

- `-nopause`
- `-singleoutput`
- `-config [ConfigName]`
- `-o [OutputDir]` or `-output [OutputDir]`
- `-c:[AnySettingName] [value]` - Force setting through the command line, **currently works only for string values.**
