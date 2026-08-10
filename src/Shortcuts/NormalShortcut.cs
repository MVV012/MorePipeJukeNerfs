using BepInEx.Logging;

namespace MorePipeJukeNerfs.Shortcuts;

public class NormalShortcut : IShortcut
{
    private ShortcutData _shortcutData;

    public NormalShortcut(ShortcutData shortcutData)
    {
        if (shortcutData.shortCutType != ShortcutData.Type.Normal)
        {
            ReleaseLog(LogLevel.Error, $"NormalShortcut was created with shortcut of type {shortcutData.shortCutType}");
        }
        _shortcutData = shortcutData;
    }

    public ShortcutData.Type Type => ShortcutData.Type.Normal;

    public AbstractRoom StartRoom => _shortcutData.room.abstractRoom;
    public AbstractRoom DestRoom => _shortcutData.room.abstractRoom;
    public WorldCoordinate StartCoord => _shortcutData.startCoord;
    public WorldCoordinate DestCoord => _shortcutData.destinationCoord;

    public override string ToString() => this.ConvertToString();
}
