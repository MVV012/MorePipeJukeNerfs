using System.Runtime.CompilerServices;

namespace MorePipeJukeNerfs.Shortcuts;

public static class VesselShortcutCWT
{
    private static ConditionalWeakTable<ShortcutHandler.ShortCutVessel, IShortcut> s_shortcuts = new();

    public static void ApplyHooks()
    {
        On.ShortcutHandler.SuckInCreature += ShortcutHandler_SuckInCreature;
        On.ShortcutHandler.CreatureEnterFromAbstractRoom += ShortcutHandler_CreatureEnterFromAbstractRoom;
    }

    public static void RemoveHooks()
    {
        On.ShortcutHandler.SuckInCreature -= ShortcutHandler_SuckInCreature;
        On.ShortcutHandler.CreatureEnterFromAbstractRoom -= ShortcutHandler_CreatureEnterFromAbstractRoom;
    }

    extension (ShortcutHandler.ShortCutVessel vessel)
    {
        public bool TryGetShortcut(out IShortcut shortcut)
        {
            return s_shortcuts.TryGetValue(vessel, out shortcut);
        }
        public IShortcut? Shortcut => vessel.TryGetShortcut(out IShortcut shortcut) ? shortcut : null;
    }

    private static void ShortcutHandler_SuckInCreature(On.ShortcutHandler.orig_SuckInCreature orig, ShortcutHandler self, Creature creature, Room room, ShortcutData shortCut)
    {
        orig(self, creature, room, shortCut);

        if (creature.Template.smallCreature)
        {
            return;
        }

        ShortcutHandler.ShortCutVessel vessel = self.transportVessels[^1];

        if (shortCut.shortCutType == ShortcutData.Type.Normal)
        {
            s_shortcuts.Add(vessel, new NormalShortcut(shortCut));
        }
        else if (shortCut.shortCutType == ShortcutData.Type.RoomExit)
        {
            s_shortcuts.Add(vessel, new ShortcutFromRealized(shortCut));
        }
    }

    private static void ShortcutHandler_CreatureEnterFromAbstractRoom(On.ShortcutHandler.orig_CreatureEnterFromAbstractRoom orig, ShortcutHandler self, Creature creature, AbstractRoom enterRoom, int enterNode)
    {
        orig(self, creature, enterRoom, enterNode);

        if (creature.Template.smallCreature)
        {
            return;
        }

        if (self.betweenRoomsWaitingLobby[^1] is ShortcutHandler.ShortCutVessel vessel)
        {
            ShortcutData shortcutData = enterRoom.realizedRoom.ShortcutLeadingToNode(enterNode);

            if (shortcutData.shortCutType == ShortcutData.Type.RoomExit)
            {
                s_shortcuts.Add(vessel, new OppositeShortcut(new ShortcutFromRealized(shortcutData)));
            }
        }
    }
}
