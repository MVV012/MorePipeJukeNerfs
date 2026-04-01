namespace MorePipeJukeNerfs.Shortcuts;

public interface IShortcut
{
    ShortcutData.Type Type { get; }

    WorldCoordinate StartCoord { get; }
    WorldCoordinate DestCoord { get; }
    AbstractRoom StartRoom { get; }
    AbstractRoom DestRoom { get; }
}

// Default interface implementation isn't supported with .net framework :(
public static class IShortcutComparison
{
    extension (IShortcut self)
    {
        public bool IsSameDirection(IShortcut other)
        {
            if (self.Type == ShortcutData.Type.Normal && other.Type == ShortcutData.Type.Normal)
            {
                return self.StartCoord == other.StartCoord && self.DestCoord == other.DestCoord;
            }
            else if (self.Type == ShortcutData.Type.RoomExit && other.Type == ShortcutData.Type.RoomExit)
            {
                return self.StartRoom == other.StartRoom && self.DestRoom == other.DestRoom;
            }
            return false;
        }

        public bool IsOppositeDirection(IShortcut other)
        {
            if (self.Type == ShortcutData.Type.Normal && other.Type == ShortcutData.Type.Normal)
            {
                return self.StartCoord == other.DestCoord && self.DestCoord == other.StartCoord;
            }
            else if (self.Type == ShortcutData.Type.RoomExit && other.Type == ShortcutData.Type.RoomExit)
            {
                return self.StartRoom == other.DestRoom && self.DestRoom == other.StartRoom;
            }
            return false;
        }

        public bool IsSameShortcut(IShortcut other)
        {
            return self.IsSameDirection(other) || self.IsOppositeDirection(other);
        }
    }
}