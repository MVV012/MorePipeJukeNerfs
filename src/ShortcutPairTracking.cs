using MorePipeJukeNerfs.Shortcuts;
using RWGenerators;

namespace MorePipeJukeNerfs;

[AttachOptionalField<ShortcutHandler.ShortCutVessel, List<AbstractCreature>>("MetCreatures", AddGetOrCreateValue = true)]
public static partial class ShortcutPairTracking
{
    public delegate void PairCreatureExitedHandler(AbstractCreature cur, AbstractCreature other, IShortcut shortcut);

    public static event PairCreatureExitedHandler? FirstCreatureExited;
    public static event PairCreatureExitedHandler? SecondCreatureExited;

    public static void OnEnable()
    {
        VesselRemovalTracking.OnShortcutVesselRemoved += OnShortcutVesselRemoved;

        Plugin.RegisterOnDisable(() => {
            VesselRemovalTracking.OnShortcutVesselRemoved -= OnShortcutVesselRemoved;
        });
    }

    private static void OnShortcutVesselRemoved(ShortcutHandler shortcuts, ShortcutHandler.ShortCutVessel vessel, bool toAbstract)
    {
        if (!Config.ShortcutNoticeCreatures)
        {
            return;
        }

        if (vessel.TryGetShortcut(out IShortcut curShortcut))
        {
            foreach (ShortcutHandler.ShortCutVessel otherVessel in GetAllShortCutVessels(shortcuts))
            {
                if (otherVessel.TryGetShortcut(out IShortcut otherShortcut)
                    && curShortcut.IsOppositeDirection(otherShortcut)
                    && ShouldPairBeTracked(vessel.creature.abstractCreature, otherVessel.creature.abstractCreature))
                {
                    LogInfo($"{vessel.creature} and {otherVessel.creature} met in shortcut");

                    FirstCreatureExited?.Invoke(vessel.creature.abstractCreature, otherVessel.creature.abstractCreature, curShortcut);

                    otherVessel.GetOrCreateMetCreatures().Add(vessel.creature.abstractCreature);
                }
            }

            if (vessel.TryGetMetCreatures(out List<AbstractCreature> metCreatures))
            {
                foreach (AbstractCreature otherCreature in metCreatures)
                {
                    SecondCreatureExited?.Invoke(vessel.creature.abstractCreature, otherCreature, curShortcut);
                }
                vessel.RemoveMetCreatures();
            }
        }
    }

    private static IEnumerable<ShortcutHandler.ShortCutVessel> GetAllShortCutVessels(ShortcutHandler shortcutHandler)
    {
        return shortcutHandler.transportVessels.Concat(shortcutHandler.betweenRoomsWaitingLobby.OfType<ShortcutHandler.ShortCutVessel>());
    }

    private static bool ShouldPairBeTracked(AbstractCreature first, AbstractCreature second)
    {
        if (Config.ShortcutNoticeOnlyPlayer
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
