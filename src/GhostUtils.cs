using System.Runtime.CompilerServices;

namespace MorePipeJukeNerfs;

public static class GhostUtils
{
    private static readonly ConditionalWeakTable<Tracker.Ghost, GhostPushable> s_ghostPushable = new();
    private class GhostPushable()
    {
        public bool Pushable { get; set; } = true;
    }

    public static void ApplyHooks()
    {
        On.Tracker.Ghost.Push += Ghost_Push;
        On.Tracker.Ghost.Reset += Ghost_Reset;
    }

    private static void Ghost_Push(On.Tracker.Ghost.orig_Push orig, Tracker.Ghost self)
    {
        if (self.Pushable)
        {
            orig(self);
        }
    }

    private static void Ghost_Reset(On.Tracker.Ghost.orig_Reset orig, Tracker.Ghost self)
    {
        orig(self);
        self.Pushable = true;
    }

    extension(Tracker.Ghost ghost)
    {
        public bool Pushable
        {
            get => s_ghostPushable.GetOrCreateValue(ghost).Pushable;
            set => s_ghostPushable.GetOrCreateValue(ghost).Pushable = value;
        }

        public void Pause()
        {
            ghost.stopped = true;
            ghost.Pushable = false;
        }

        public void Unpause()
        {
            ghost.stopped = false;
            ghost.Pushable = true;
        }
    }
}
