using MorePipeJukeNerfs.Shortcuts;
using RWCustom;

namespace MorePipeJukeNerfs;

internal static class ShortcutCounterReducer
{
    public static void ApplyHooks()
    {
        On.ShortcutHandler.SpitOutCreature += ShortcutHandler_SpitOutCreature;
    }

    private static void ShortcutHandler_SpitOutCreature(On.ShortcutHandler.orig_SpitOutCreature orig, ShortcutHandler self, ShortcutHandler.ShortCutVessel vessel)
    {
        orig(self, vessel);

        if (!Config.ReduceInvincibility && !Config.IncreaseShortcutDelay)
        {
            return;
        }

        if (vessel.creature is Player { isNPC: false } player && vessel.TryGetShortcut(out IShortcut shortcut))
        {
            int shortcutUses = player.ShortcutUsesTracker.ExitedShortcut(shortcut);

            if (Config.ReduceInvincibility)
            {
                int invincibility = GetNewRoomInvincibility(shortcutUses);
                player.newToRoomInvinsibility = invincibility;
                player.cantBeGrabbedCounter = Custom.IntClamp(invincibility, 0, 30);
            }
            if (Config.IncreaseShortcutDelay)
            {
                player.shortcutDelay = GetShortcutDelay(shortcutUses);
            }
        }
    }

    private static int GetNewRoomInvincibility(int repeatingShortcutCount)
    {
        if (repeatingShortcutCount < Config.InvincibilityShortcutUses)
        {
            return Config.InvincibilityStarting;
        }
        else
        {
            return Custom.IntClamp(
                Config.InvincibilityStarting - (repeatingShortcutCount - Config.InvincibilityShortcutUses + 1) * Config.InvincibilityReduction,
                Config.InvincibilityMin,
                Config.InvincibilityStarting
            );
        }
    }

    private static int GetShortcutDelay(int repeatingShortcutCount)
    {
        if (repeatingShortcutCount < Config.ShortcutDelayShortcutUses)
        {
            return Config.ShortcutDelayStarting;
        }
        else
        {
            return Custom.IntClamp(
                Config.ShortcutDelayStarting + (repeatingShortcutCount - Config.ShortcutDelayShortcutUses + 1) * Config.ShortcutDelayIncrease,
                Config.ShortcutDelayStarting,
                Config.ShortcutDelayMax
            );
        }
    }
}
