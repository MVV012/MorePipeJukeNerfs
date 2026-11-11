using RWGenerators;

namespace MorePipeJukeNerfs.Debug.Tests;

// These will be cleared on WorldLoaded and RWGameShutDown
[AttachField<AbstractRoom, bool>("KeepRealized", false, Impl = AttachFieldImpl.StrongRef)]
[AttachField<AbstractRoom, bool>("KeepAbstract", false, Impl = AttachFieldImpl.StrongRef)]
internal static partial class RoomRealizingRestrictions
{
    extension(AbstractRoom room)
    {
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
        AbstractRoom.ClearAllKeepRealizedFields();
        AbstractRoom.ClearAllKeepAbstractFields();
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
