using RWGenerators;

namespace MorePipeJukeNerfs.Shortcuts;

[AttachOptionalField<ShortcutHandler.ShortCutVessel, IShortcut>("Shortcut")]
public static partial class VesselShortcutInfo
{
    public static void ApplyHooks()
    {
        On.ShortcutHandler.SuckInCreature += ShortcutHandler_SuckInCreature;
        On.ShortcutHandler.CreatureEnterFromAbstractRoom += ShortcutHandler_CreatureEnterFromAbstractRoom;
    }

    private static void ShortcutHandler_SuckInCreature(On.ShortcutHandler.orig_SuckInCreature orig, ShortcutHandler self, Creature creature, Room room, ShortcutData shortCut)
    {
        orig(self, creature, room, shortCut);

        if (creature.Template.smallCreature)
        {
            return;
        }

        if (self.transportVessels.Count == 0)
        {
            LogWarning("ShortcutHandler_SuckInCreature: transportVessels is empty for some reason");
            return;
        }
        ShortcutHandler.ShortCutVessel vessel = self.transportVessels[^1];
        if (vessel.creature != creature)
        {
            LogWarning("ShortcutHandler_SuckInCreature: " +
                "Last ShortcutVessel in transportVessels has different creature for some reason");
            return;
        }

        if (shortCut.shortCutType == ShortcutData.Type.Normal)
        {
            vessel.SetShortcut(new NormalShortcut(shortCut));
        }
        else if (shortCut.shortCutType == ShortcutData.Type.RoomExit)
        {
            vessel.SetShortcut(new ShortcutFromRealized(shortCut));
        }
    }

    private static void ShortcutHandler_CreatureEnterFromAbstractRoom(On.ShortcutHandler.orig_CreatureEnterFromAbstractRoom orig, ShortcutHandler self, Creature creature, AbstractRoom enterRoom, int enterNode)
    {
        orig(self, creature, enterRoom, enterNode);

        if (creature.Template.smallCreature)
        {
            return;
        }

        if (self.betweenRoomsWaitingLobby.Count == 0)
        {
            LogWarning("ShortcutHandler_CreatureEnterFromAbstractRoom: " +
                "betweenRoomsWaitingLobby is empty for some reason");
            return;
        }
        if (self.betweenRoomsWaitingLobby[^1] is ShortcutHandler.ShortCutVessel vessel)
        {
            if (vessel.creature != creature)
            {
                LogWarning("ShortcutHandler_CreatureEnterFromAbstractRoom: " +
                    "Last ShortcutVessel in betweenRoomsWaitingLobby has different creature for some reason");
                return;
            }

            ShortcutData shortcutData = enterRoom.realizedRoom.ShortcutLeadingToNode(enterNode);

            if (shortcutData.shortCutType == ShortcutData.Type.RoomExit)
            {
                vessel.SetShortcut(new OppositeShortcut(new ShortcutFromRealized(shortcutData)));
            }
        }
    }
}
