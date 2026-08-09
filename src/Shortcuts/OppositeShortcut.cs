namespace MorePipeJukeNerfs.Shortcuts;

public class OppositeShortcut : IShortcut
{
    private IShortcut _shortcut;

    public OppositeShortcut(IShortcut shortcut)
    {
        _shortcut = shortcut;
    }

    public ShortcutData.Type Type => _shortcut.Type;

    public WorldCoordinate StartCoord => _shortcut.DestCoord;
    public WorldCoordinate DestCoord => _shortcut.StartCoord;
    public AbstractRoom StartRoom => _shortcut.DestRoom;
    public AbstractRoom DestRoom => _shortcut.StartRoom;

    public override string ToString() => this.ConvertToString();
}
