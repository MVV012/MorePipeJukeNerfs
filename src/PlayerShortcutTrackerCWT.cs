using System.Runtime.CompilerServices;

namespace MorePipeJukeNerfs;

public static class PlayerShortcutTrackerCWT
{
    private static readonly ConditionalWeakTable<Player, RepeatingShortcutTracker> s_repeatingShortcutTrackers = new();

    extension(Player player)
    {
        public RepeatingShortcutTracker ShortcutTracker => s_repeatingShortcutTrackers.GetOrCreateValue(player);
    }

    public static void ApplyHooks()
    {
        On.Player.Update += Player_Update;
    }

    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        self.ShortcutTracker.Update();
    }
}
