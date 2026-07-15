namespace MorePipeJukeNerfs.Shortcuts;

public class ShortcutFromRealized : IShortcut
{
    private AbstractRoom _startRoom;
    private AbstractRoom? _destRoom;
    private WorldCoordinate _startCoord;
    private WorldCoordinate? _destCoord;

    public ShortcutFromRealized(ShortcutData shortcutData)
    {
        if (shortcutData.shortCutType != ShortcutData.Type.RoomExit)
        {
            Log.LogError($"ShortcutFromRealized was created with shortcut of type {shortcutData.shortCutType}");
        }

        _startRoom = shortcutData.room.abstractRoom;
        _destRoom = _startRoom.world.GetAbstractRoom(_startRoom.connections[shortcutData.destNode]);
        _startCoord = shortcutData.startCoord;
        _startCoord.abstractNode = shortcutData.destNode;
        _ = this.DestCoord; // Attempt to cache other side coord
    }

    public ShortcutData.Type Type => ShortcutData.Type.RoomExit;

    public AbstractRoom StartRoom => _startRoom;
    public AbstractRoom DestRoom => _destRoom ?? _startRoom; // idk
    public WorldCoordinate StartCoord => _startCoord;
    public WorldCoordinate DestCoord
    {
        get
        {
            if (_destCoord.HasValue) return _destCoord.Value;

            if (_destRoom == null)
            {
                _destCoord = new WorldCoordinate(_startRoom.index, -1, -1, -1);
                return _destCoord.Value;
            }

            // This code is copied from Tracker.Ghost.Update
            WorldCoordinate dest = new WorldCoordinate(_destRoom.index, -1, -1, _destRoom.ExitIndex(_startRoom.index));
            if (_destRoom.realizedRoom != null && _destRoom.realizedRoom.shortCutsReady)
            {
                dest.Tile = _destRoom.realizedRoom.ShortcutLeadingToNode(dest.abstractNode).StartTile;
                _destCoord = dest;
            }
            return dest;
        }
    }
}
