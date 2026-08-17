using System.Runtime.CompilerServices;

namespace MorePipeJukeNerfs.Debug.Tests;

internal static class RoomRealizingRestrictions
{
    private static ConditionalWeakTable<AbstractRoom, RealizingRestrictions> s_realizingRestrictions = new();

    public class RealizingRestrictions
    {
        public bool KeepRealized = false;
        public bool KeepAbstract = false;
    }

    extension(AbstractRoom room)
    {
        public bool KeepRealized
        {
            get => s_realizingRestrictions.GetOrCreateValue(room).KeepRealized;
            set => s_realizingRestrictions.GetOrCreateValue(room).KeepRealized = value;
        }

        public bool KeepAbstract
        {
            get => s_realizingRestrictions.GetOrCreateValue(room).KeepAbstract;
            set => s_realizingRestrictions.GetOrCreateValue(room).KeepAbstract = value;
        }

        public void AbstractizeAndRestrict()
        {
            room.KeepRealized = false;
            room.Abstractize();
            room.KeepAbstract = true;
        }

        public void RealizeAndRestrict(World world, RainWorldGame game)
        {
            room.KeepAbstract = false;
            room.RealizeRoom(world, game);
            room.KeepRealized = true;
        }

        public void RemoveRealizingRestrictions()
        {
            room.KeepRealized = false;
            room.KeepAbstract = false;
        }
    }

    public static void RemoveAllRestrictions()
    {
        s_realizingRestrictions = new();
    }

    public static void ApplyHooks()
    {
        On.AbstractRoom.RealizeRoom += AbstractRoom_RealizeRoom;
        On.AbstractRoom.Abstractize += AbstractRoom_Abstractize;
    }

    private static void AbstractRoom_RealizeRoom(On.AbstractRoom.orig_RealizeRoom orig, AbstractRoom self, World world, RainWorldGame game)
    {
        if (!self.KeepAbstract)
        {
            orig(self, world, game);
        }
    }

    private static void AbstractRoom_Abstractize(On.AbstractRoom.orig_Abstractize orig, AbstractRoom self)
    {
        if (!self.KeepRealized)
        {
            orig(self);
        }
    }
}
