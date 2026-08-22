using MorePipeJukeNerfs;
using MorePipeJukeNerfs.Shortcuts;

namespace MorePipeJukeNerfs;

internal class PipeJukeNotifier
{
    public static void OnEnable()
    {
        ShortcutPairTracking.FirstCreatureExited += FirstCreatureExited;
        ShortcutPairTracking.SecondCreatureExited += SecondCreatureExited;

        Plugin.OnDisableEvent += OnDisable;
    }

    public static void OnDisable()
    {
        ShortcutPairTracking.FirstCreatureExited -= FirstCreatureExited;
        ShortcutPairTracking.SecondCreatureExited -= SecondCreatureExited;
    }

    private static void FirstCreatureExited(AbstractCreature cur, AbstractCreature other, IShortcut shortcut)
    {
        DebugLogInfo($"First exited: {cur} (other: {other})");

        OnPipeJuke(cur, other, shortcut.StartRoom, shortcut.StartCoord, pause: true, setForbiddenRoomExit: shortcut.Type == ShortcutData.Type.RoomExit);

        // Updating state here is more accurate, but causes exceptions for some creatures (curently found: EggBug, BigNeedleWorm)
        OnPipeJuke(other, cur, shortcut.DestRoom, shortcut.DestCoord, pause: false, setForbiddenRoomExit: shortcut.Type == ShortcutData.Type.RoomExit);
    }

    private static void SecondCreatureExited(AbstractCreature cur, AbstractCreature other, IShortcut shortcut)
    {
        DebugLogInfo($"Second exited: {cur} (other: {other})");

        if (other.TryGetRepresentation(cur, out var rep))
        {
            rep.UnpauseUnpushableGhosts();

            if (!rep.lastSeenCoord.TileDefined && shortcut.DestCoord.TileDefined)
            {
                rep.MoveToShortcutEntrance(shortcut.DestRoom, shortcut.DestCoord, pause: false, setForbiddenRoomExit: shortcut.Type == ShortcutData.Type.RoomExit);
            }
        }

        // Updating state here is less accurate, but causes less exceptions
        //OnPipeJuke(cur, other, shortcut.StartRoom, shortcut.StartCoord, pause: false, setForbiddenRoomExit: shortcut.Type == ShortcutData.Type.RoomExit);
    }

    private static void OnPipeJuke(AbstractCreature cur, AbstractCreature other, AbstractRoom otherRoom, WorldCoordinate otherCoord, bool pause = false, bool setForbiddenRoomExit = false)
    {
        try
        {
            CreatureFixes.PipeJuking = true;

            Tracker.CreatureRepresentation? rep;
            if (cur.TryGetRepresentation(other, out rep))
            {
                if (Options.ShortcutNoticeSeen.Value)
                {
                    rep.MoveToShortcutEntrance(otherRoom, otherCoord, pause, setForbiddenRoomExit);
                    rep.UpdateStateAndRelationship();
                }
            }
            else
            {
                if (Options.ShortcutNoticeUnseen.Value)
                {
                    cur.NoticeCreature(other);
                    if (cur.TryGetRepresentation(other, out rep))
                    {
                        rep.MoveToShortcutEntrance(otherRoom, otherCoord, pause, setForbiddenRoomExit);
                        rep.UpdateStateAndRelationship();
                    }
                }
            }
        }
        finally
        {
            CreatureFixes.PipeJuking = false;
        }
    }
}
