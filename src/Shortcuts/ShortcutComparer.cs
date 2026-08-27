using RWCustom;

namespace MorePipeJukeNerfs.Shortcuts;

public class ShortcutComparer : EqualityComparer<IShortcut>
{
    public static ShortcutComparer SameShortcut = new(ignoreDirection: true);
    public static ShortcutComparer SameDirection = new(ignoreDirection: false);

    public bool IgnoreDirection { get; }

    public ShortcutComparer(bool ignoreDirection)
    {
        IgnoreDirection = ignoreDirection;
    }

    public override bool Equals(IShortcut x, IShortcut y)
    {
        if (IgnoreDirection)
        {
            return x.IsSameShortcut(y);
        }
        else
        {
            return x.IsSameDirection(y);
        }
    }

    public override int GetHashCode(IShortcut x)
    {
        if (x.Type == ShortcutData.Type.Normal)
        {
            int room = x.StartRoom.index;
            IntVector2 tile1 = x.StartCoord.Tile, tile2 = x.DestCoord.Tile;

            if (IgnoreDirection && (tile1.x > tile2.x || (tile1.x == tile2.x && tile1.y > tile2.y)))
            {
                (tile2, tile1) = (tile1, tile2);
            }

            return (x.Type, room, tile1.x, tile1.y, tile2.x, tile2.y).GetHashCode();
        }
        else if (x.Type == ShortcutData.Type.RoomExit)
        {
            int room1 = x.StartRoom.index, room2 = x.DestRoom.index;

            if (IgnoreDirection && room1 > room2)
            {
                (room1, room2) = (room2, room1);
            }

            return (x.Type, room1, room2).GetHashCode();
        }
        else
        {
            return 0;
        }
    }
}
