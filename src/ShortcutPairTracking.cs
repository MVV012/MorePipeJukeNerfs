using MonoMod.Cil;
using BepInEx.Logging;
using MorePipeJukeNerfs.Shortcuts;
using System.Runtime.CompilerServices;
using MonoMod.Utils;

namespace MorePipeJukeNerfs;

public static class ShortcutPairTracking
{
    private static ConditionalWeakTable<ShortcutHandler.ShortCutVessel, List<AbstractCreature>> s_metCreatures = new();

    public delegate void PairCreatureExitedHandler(AbstractCreature cur, AbstractCreature other, IShortcut shortcut);

    public static event PairCreatureExitedHandler? FirstCreatureExited;
    public static event PairCreatureExitedHandler? SecondCreatureExited;

    public static void ApplyHooks()
    {
        On.ShortcutHandler.SpitOutCreature += ShortcutHandler_SpitOutCreature;
        IL.ShortcutHandler.Update += ShortcutHandler_Update_ILHook;
    }

    private static void ShortcutHandler_SpitOutCreature(On.ShortcutHandler.orig_SpitOutCreature orig, ShortcutHandler self, ShortcutHandler.ShortCutVessel vessel)
    {
        orig(self, vessel);

        OnShortcutVesselRemoved(self, vessel);
    }

    private static void ShortcutHandler_Update_ILHook(ILContext il)
    {
        try
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(
                x => x.MatchCallvirt<AbstractWorldEntity>(nameof(AbstractWorldEntity.Abstractize))
            );

            // betweenRoomsWaitingLobby[num5]
            c.GotoPrev(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<ShortcutHandler>(nameof(ShortcutHandler.betweenRoomsWaitingLobby)),
                x => x.MatchLdloc(8),
                x => x.MatchCallvirt(typeof(List<ShortcutHandler.Vessel>).GenericName, "get_Item")
            );
            c.MoveAfterLabels();

            c.EmitDelegate(BeforeBetweenRoomsVesselRemoval);
        }
        catch (Exception e)
        {
            ReleaseLog(LogLevel.Error, $"Failed to apply IL hook to ShortcutHandler.Update(). Exception: {e}");
        }

        static ShortcutHandler.Vessel BeforeBetweenRoomsVesselRemoval(ShortcutHandler.Vessel vessel)
        {
            if (vessel is ShortcutHandler.ShortCutVessel shortcutVessel)
            {
                OnShortcutVesselRemoved(shortcutVessel.room.world.game.shortcuts, shortcutVessel);
            }

            return vessel;
        }
    }

    extension(Type type)
    {
        public string GenericName => type.ToString().Replace("[", "<").Replace("]", ">").Replace("+", "/");
    }

    private static void OnShortcutVesselRemoved(ShortcutHandler self, ShortcutHandler.ShortCutVessel vessel)
    {
        if (!Options.ShortcutNoticeCreatures.Value)
        {
            return;
        }

        if (vessel.TryGetShortcut(out IShortcut curShortcut))
        {
            foreach (ShortcutHandler.ShortCutVessel otherVessel in GetAllShortCutVessels(self))
            {
                if (otherVessel.TryGetShortcut(out IShortcut otherShortcut)
                    && curShortcut.IsOppositeDirection(otherShortcut)
                    && ShouldPairBeTracked(vessel.creature.abstractCreature, otherVessel.creature.abstractCreature))
                {
                    ReleaseLog(LogLevel.Info, $"{vessel.creature} and {otherVessel.creature} met in shortcut");

                    FirstCreatureExited?.Invoke(vessel.creature.abstractCreature, otherVessel.creature.abstractCreature, curShortcut);

                    s_metCreatures.GetOrCreateValue(otherVessel).Add(vessel.creature.abstractCreature);
                }
            }

            foreach (AbstractCreature otherCreature in s_metCreatures.GetOrCreateValue(vessel))
            {
                SecondCreatureExited?.Invoke(vessel.creature.abstractCreature, otherCreature, curShortcut);
            }
        }
    }

    private static IEnumerable<ShortcutHandler.ShortCutVessel> GetAllShortCutVessels(ShortcutHandler shortcutHandler)
    {
        return shortcutHandler.transportVessels.Concat(shortcutHandler.betweenRoomsWaitingLobby.OfType<ShortcutHandler.ShortCutVessel>());
    }

    private static bool ShouldPairBeTracked(AbstractCreature first, AbstractCreature second)
    {
        if (Options.ShortcutNoticeOnlyPlayer.Value
            && first.realizedCreature is not Player { isNPC: false }
            && second.realizedCreature is not Player { isNPC: false })
        {
            return false;
        }
        if (first.rippleLayer != second.rippleLayer && !first.rippleBothSides && !second.rippleBothSides)
        {
            return false;
        }
        if (first.realizedCreature is Player { IsHidden: true, VisibilityBonus: <= -1f }
            || second.realizedCreature is Player { IsHidden: true, VisibilityBonus: <= -1f })
        {
            return false;
        }
        return true;
    }
}
