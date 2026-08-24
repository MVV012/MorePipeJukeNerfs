using BepInEx.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MorePipeJukeNerfs.Shortcuts;

namespace MorePipeJukeNerfs;

public static class VesselRemovalTracking
{
    public delegate void ShortcutVesselRemovalHandler(ShortcutHandler shortcuts, ShortcutHandler.ShortCutVessel vessel, bool toAbstract);
    public static event ShortcutVesselRemovalHandler? OnShortcutVesselRemoved;

    public static void ApplyHooks()
    {
        On.ShortcutHandler.SpitOutCreature += ShortcutHandler_SpitOutCreature;
        IL.ShortcutHandler.Update += ShortcutHandler_Update_ILHook;
    }

    private static void ShortcutHandler_SpitOutCreature(On.ShortcutHandler.orig_SpitOutCreature orig, ShortcutHandler self, ShortcutHandler.ShortCutVessel vessel)
    {
        orig(self, vessel);

        OnShortcutVesselRemoved?.Invoke(self, vessel, toAbstract: false);
    }

    private static void ShortcutHandler_Update_ILHook(ILContext il)
    {
        try
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(x => x.MatchGetter<ShortcutHandler.BorderVessel>(nameof(ShortcutHandler.BorderVessel.Arrived)));

            // transportVessels.RemoveAt(num);
            c.GotoPrev(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<ShortcutHandler>(nameof(ShortcutHandler.transportVessels)),
                x => x.MatchLdloc(0),
                x => x.MatchCallvirt(typeof(List<ShortcutHandler.ShortCutVessel>).GenericName, nameof(List<>.RemoveAt))
            );
            c.MoveAfterLabels();

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc_0);
            c.EmitDelegate(BeforeTransportVesselRemoval);

            static void BeforeTransportVesselRemoval(ShortcutHandler self, int i)
            {
                OnShortcutVesselRemoved?.Invoke(self, self.transportVessels[i], toAbstract: true);
            }


            c.GotoNext(x => x.MatchCallvirt<AbstractWorldEntity>(nameof(AbstractWorldEntity.Abstractize)));

            // betweenRoomsWaitingLobby[num5]
            c.GotoPrev(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<ShortcutHandler>(nameof(ShortcutHandler.betweenRoomsWaitingLobby)),
                x => x.MatchLdloc(8),
                x => x.MatchCallvirt(typeof(List<ShortcutHandler.Vessel>).GenericName, "get_Item")
            );
            c.MoveAfterLabels();

            c.EmitDelegate(BeforeBetweenRoomsVesselRemoval);

            static ShortcutHandler.Vessel BeforeBetweenRoomsVesselRemoval(ShortcutHandler.Vessel vessel)
            {
                if (vessel is ShortcutHandler.ShortCutVessel shortcutVessel)
                {
                    OnShortcutVesselRemoved?.Invoke(shortcutVessel.room.world.game.shortcuts, shortcutVessel, toAbstract: true);
                }

                return vessel;
            }
        }
        catch (Exception e)
        {
            ReleaseLog(LogLevel.Error, $"Failed to apply IL hook to ShortcutHandler.Update!!! Exception: {e}");
        }
    }
}
