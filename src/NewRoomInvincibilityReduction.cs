using MorePipeJukeNerfs.Shortcuts;

namespace MorePipeJukeNerfs;

internal static class NewRoomInvincibilityReduction
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
            int value = CalculateNewRoomInvincibility(player.ShortcutTracker.RepeatingShortcutCount);
            player.newToRoomInvinsibility = Math.Min(value, 40);
            player.cantBeGrabbedCounter = Math.Min(value, 30);
        }
    }

    private static int CalculateNewRoomInvincibility(int repeatingShortcutCount)
    {
        // TODO: move to plugin options
        int startingValue = 40;
        int reduction = 10;

        return Math.Max(startingValue - (repeatingShortcutCount - 1) * reduction, 0);
    }
}
