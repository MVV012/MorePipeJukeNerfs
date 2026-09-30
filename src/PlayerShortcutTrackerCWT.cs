using System.Runtime.CompilerServices;

namespace MorePipeJukeNerfs;

public static class PlayerShortcutTrackerCWT
{
    internal static ConditionalWeakTable<Player, ShortcutUsesTracker> s_shortcutUsesTrackers = new();

    extension(Player player)
    {
        public ShortcutUsesTracker ShortcutUsesTracker => s_shortcutUsesTrackers.GetOrCreateValue(player);
    }

    public static void ApplyHooks()
    {
        On.Player.Update += Player_Update;
    }

    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        self.ShortcutUsesTracker.Update();
    }
}
