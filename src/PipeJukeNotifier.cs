using MorePipeJukeNerfs.Shortcuts;

namespace MorePipeJukeNerfs;

internal class PipeJukeNotifier
{
    public static void ApplyHooks() // Rename?
    {
        ShortcutPairTracking.FirstCreatureExited += FirstCreatureExited;
        ShortcutPairTracking.SecondCreatureExited += SecondCreatureExited;
    }

    public static void RemoveHooks()
    {
        ShortcutPairTracking.FirstCreatureExited -= FirstCreatureExited;
        ShortcutPairTracking.SecondCreatureExited -= SecondCreatureExited;
    }

    private static void FirstCreatureExited(AbstractCreature cur, AbstractCreature other, Shortcuts.IShortcut shortcut)
    {
        DebugLogInfo($"First exited: {cur} (other: {other})");

        OnPipeJuke(cur, other, shortcut.StartRoom, shortcut.StartCoord, stop: true);

        // Updating state here is more accurate, but causes exceptions for some creatures (curently found: EggBug, BigNeedleWorm)
        OnPipeJuke(other, cur, shortcut.DestRoom, shortcut.DestCoord, stop: false);
    }

    private static void SecondCreatureExited(AbstractCreature cur, AbstractCreature other, Shortcuts.IShortcut shortcut)
    {
        DebugLogInfo($"Second exited: {cur} (other: {other})");

        if (other.TryGetRepresentation(cur, out var rep))
        {
            rep.UnpauseStoppedGhost();
        }

        // Updating state here is less accurate, but causes less exceptions
        //OnPipeJuke(cur, other, shortcut.StartRoom, shortcut.StartCoord, stop: false);
    }

    private static void OnPipeJuke(AbstractCreature cur, AbstractCreature other, AbstractRoom otherRoom, WorldCoordinate otherCoord, bool stop = false)
    {
        if (Options.ShortcutNoticeOnlyPlayer.Value && other.realizedCreature is not Player { isNPC: false })
        {
            return;
        }
        if (other.realizedCreature is Player { IsHidden: true, VisibilityBonus: <= -1f })
        {
            return;
        }

        Tracker.CreatureRepresentation? rep;
        if (cur.TryGetRepresentation(other, out rep))
        {
            if (Options.ShortcutNoticeSeen.Value)
            {
                rep.MoveToShortcutEntrance(otherRoom, otherCoord, stop);
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
                    rep.MoveToShortcutEntrance(otherRoom, otherCoord, stop);
                }
            }
        }
    }
}
