using MorePipeJukeNerfs.Shortcuts;
using RWCustom;

namespace MorePipeJukeNerfs;

internal static class ShortcutCounterReducer
{
    public static void ApplyHooks()
    {
        On.ShortcutHandler.SpitOutCreature += ShortcutHandler_SpitOutCreature;
    }

    public static void RemoveHooks()
    {
        On.ShortcutHandler.SpitOutCreature -= ShortcutHandler_SpitOutCreature;
    }

    private static void ShortcutHandler_SpitOutCreature(On.ShortcutHandler.orig_SpitOutCreature orig, ShortcutHandler self, ShortcutHandler.ShortCutVessel vessel)
    {
        orig(self, vessel);

        if (vessel.creature is Player player && vessel.TryGetShortcut(out IShortcut shortcut))
        {
            player.ShortcutTracker.ExitedShortcut(shortcut);

            int invincibility = GetNewRoomInvincibility(player.ShortcutTracker.RepeatingShortcutCount);
            player.newToRoomInvinsibility = invincibility;
            player.cantBeGrabbedCounter = Custom.IntClamp(invincibility, 0, 30);
            player.shortcutDelay = GetShortcutDelay(player.ShortcutTracker.RepeatingShortcutCount);
        }
    }

    private static int GetNewRoomInvincibility(int repeatingShortcutCount)
    {
        // TODO: move to plugin options
        int startingValue = 40;
        int decreaseFrom = 3;
        int reduction = 10;

        if (repeatingShortcutCount < decreaseFrom)
        {
            return startingValue;
        }
        else
        {
            return Custom.IntClamp(startingValue - (repeatingShortcutCount - decreaseFrom + 1) * reduction, 0, startingValue);
        }
    }

    private static int GetShortcutDelay(int repeatingShortcutCount)
    {
        // TODO: move to plugin options
        int startingValue = 20;
        int increaseFrom = 5;
        int increase = 10;
        int limit = 80;

        if (repeatingShortcutCount < increaseFrom)
        {
            return startingValue;
        }
        else
        {
            return Custom.IntClamp(startingValue + (repeatingShortcutCount - increaseFrom + 1) * increase, startingValue, limit);
        }
    }
}
