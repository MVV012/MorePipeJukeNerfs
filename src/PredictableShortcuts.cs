using MorePipeJukeNerfs.Shortcuts;
using RWGenerators;

namespace MorePipeJukeNerfs;

[AttachOptionalField<ShortcutHandler.ShortCutVessel, List<AbstractCreature>>("SeenByCreatures", AddGetOrCreateValue = true)]
public static partial class PredictableShortcuts
{
    public static void OnEnable()
    {
        On.Tracker.ElaborateCreatureRepresentation.LostVisualContact += ElaborateCreatureRepresentation_LostVisualContact;
        VesselRemovalTracking.OnShortcutVesselRemoved += OnShortcutVesselRemoved;

        Plugin.OnDisableEvent += () => VesselRemovalTracking.OnShortcutVesselRemoved -= OnShortcutVesselRemoved;
    }

    private static void ElaborateCreatureRepresentation_LostVisualContact(On.Tracker.ElaborateCreatureRepresentation.orig_LostVisualContact orig, Tracker.ElaborateCreatureRepresentation self)
    {
        orig(self);

        if (!Config.PredictableShortcuts)
        {
            return;
        }

        Creature? creature = self.representedCreature.realizedCreature;
        if (creature != null
            && (!Config.PredictableShortcutsOnlyPlayer || creature is Player { isNPC: false })
            && creature.inShortcut
            && creature.inShortcutVessel != null
            && creature.inShortcutVessel.TryGetShortcut(out IShortcut shortcut))
        {
            self.MoveToShortcutEntrance(shortcut.DestRoom, shortcut.DestCoord, pause: true, setForbiddenRoomExit: shortcut.Type == ShortcutData.Type.RoomExit);
            creature.inShortcutVessel.GetOrCreateSeenByCreatures().Add(self.parent.AI.creature);
        }
    }

    private static void OnShortcutVesselRemoved(ShortcutHandler shortcuts, ShortcutHandler.ShortCutVessel vessel, bool toAbstract)
    {
        if (!Config.PredictableShortcuts)
        {
            return;
        }

        if (vessel.TryGetSeenByCreatures(out List<AbstractCreature> seenBy) && vessel.TryGetShortcut(out IShortcut shortcut))
        {
            foreach (AbstractCreature creature in seenBy)
            {
                if (creature.TryGetRepresentation(vessel.creature.abstractCreature, out var rep))
                {
                    rep.CreatureExitedShortcut(shortcut.DestRoom, shortcut.DestCoord, shortcut.Type == ShortcutData.Type.RoomExit);
                }
            }
        }
    }
}
