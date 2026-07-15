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
        // TODO: move to plugin options
        bool unawareCreaturesNotice = true;
        bool awareCreaturesNotice = true;

        DebugLogInfo($"First exited: {cur} (other: {other})");

        Tracker.CreatureRepresentation? rep;
        if (cur.TryGetRepresentation(other, out rep))
        {
            if (awareCreaturesNotice)
            {
                rep.MoveToShortcutEntrance(shortcut.StartRoom, shortcut.StartCoord, stop: true);
                rep.UpdateStateAndRelationship();
            }
        }
        else
        {
            if (unawareCreaturesNotice)
            {
                cur.NoticeCreature(other);
                if (cur.TryGetRepresentation(other, out rep))
                {
                    rep.MoveToShortcutEntrance(shortcut.StartRoom, shortcut.StartCoord, stop: true);
                }
            }
        }

        // Updating state here is more accurate, but causes exceptions for some creatures (curently found: EggBug, BigNeedleWorm)
        if (other.TryGetRepresentation(cur, out rep))
        {
            if (awareCreaturesNotice)
            {
                rep.MoveToShortcutEntrance(shortcut.DestRoom, shortcut.DestCoord);
                rep.UpdateStateAndRelationship();
            }
        }
        else
        {
            if (unawareCreaturesNotice)
            {
                other.NoticeCreature(cur);
                if (other.TryGetRepresentation(cur, out rep))
                {
                    rep.MoveToShortcutEntrance(shortcut.DestRoom, shortcut.DestCoord);
                }
            }
        }
    }

    private static void SecondCreatureExited(AbstractCreature cur, AbstractCreature other, Shortcuts.IShortcut shortcut)
    {
        DebugLogInfo($"Second exited: {cur} (other: {other})");

        Tracker.CreatureRepresentation? rep;
        if (other.TryGetRepresentation(cur, out rep))
        {
            rep.UnpauseStoppedGhost();
        }

        // Updating state here is less accurate, but causes less exceptions
        /*
        bool unawareCreaturesNotice = true;
        bool awareCreaturesNotice = true;
        if (cur.TryGetRepresentation(other, out rep))
        {
            if (awareCreaturesNotice)
            {
                rep.MoveToShortcutEntrance(shortcut.StartRoom, shortcut.StartCoord);
                rep.UpdateStateAndRelationship();
            }
        }
        else
        {
            if (unawareCreaturesNotice)
            {
                cur.NoticeCreature(other);
                if (cur.TryGetRepresentation(other, out rep))
                {
                    rep.MoveToShortcutEntrance(shortcut.StartRoom, shortcut.StartCoord);
                }
            }
        }
        */
    }
}
