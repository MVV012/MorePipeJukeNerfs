using MorePipeJukeNerfs.Shortcuts;
using RWCustom;
using static MorePipeJukeNerfs.Options;

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

        if (!ReduceInvincibility.Value && !IncreaseShortcutDelay.Value)
        {
            return;
        }

        if (vessel.creature is Player { isNPC: false } player && vessel.TryGetShortcut(out IShortcut shortcut))
        {
            int shortcutUses = player.ShortcutUsesTracker.ExitedShortcut(shortcut);

            if (ReduceInvincibility.Value)
            {
                int invincibility = GetNewRoomInvincibility(shortcutUses);
                player.newToRoomInvinsibility = invincibility;
                player.cantBeGrabbedCounter = Custom.IntClamp(invincibility, 0, 30);
            }
            if (IncreaseShortcutDelay.Value)
            {
                player.shortcutDelay = GetShortcutDelay(shortcutUses);
            }
        }
    }

    private static int GetNewRoomInvincibility(int repeatingShortcutCount)
    {
        if (repeatingShortcutCount < InvincibilityShortcutUses.Value)
        {
            return InvincibilityStarting.Value;
        }
        else
        {
            return Custom.IntClamp(
                InvincibilityStarting.Value - (repeatingShortcutCount - InvincibilityShortcutUses.Value + 1) * InvincibilityReduction.Value,
                InvincibilityMin.Value,
                InvincibilityStarting.Value
            );
        }
    }

    private static int GetShortcutDelay(int repeatingShortcutCount)
    {
        if (repeatingShortcutCount < ShortcutDelayShortcutUses.Value)
        {
            return ShortcutDelayStarting.Value;
        }
        else
        {
            return Custom.IntClamp(
                ShortcutDelayStarting.Value + (repeatingShortcutCount - ShortcutDelayShortcutUses.Value + 1) * ShortcutDelayIncrease.Value,
                ShortcutDelayStarting.Value,
                ShortcutDelayMax.Value
            );
        }
    }
}
