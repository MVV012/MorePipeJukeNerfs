using System.Runtime.CompilerServices;

namespace MorePipeJukeNerfs;

public static class PlayerShortcutTrackerCWT
{
    private static ConditionalWeakTable<Player, ShortcutUsesTracker> s_shortcutUsesTrackers = new();

    extension(Player player)
    {
        public ShortcutUsesTracker ShortcutUsesTracker => s_shortcutUsesTrackers.GetOrCreateValue(player);
    }

    public static void ApplyHooks()
    {
        On.Player.Update += Player_Update;
        On.OverWorld.WorldLoaded += OverWorld_WorldLoaded;
    }

    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        self.ShortcutUsesTracker.Update();
    }

    private static void OverWorld_WorldLoaded(On.OverWorld.orig_WorldLoaded orig, OverWorld self, bool warpUsed)
    {
        orig(self, warpUsed);

        s_shortcutUsesTrackers = new();
    }
}
