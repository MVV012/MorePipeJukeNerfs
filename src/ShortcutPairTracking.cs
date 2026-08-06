using MorePipeJukeNerfs.Shortcuts;
using System.Runtime.CompilerServices;

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
    }

    public static void RemoveHooks()
    {
        On.ShortcutHandler.SpitOutCreature -= ShortcutHandler_SpitOutCreature;
    }

    private static void ShortcutHandler_SpitOutCreature(On.ShortcutHandler.orig_SpitOutCreature orig, ShortcutHandler self, ShortcutHandler.ShortCutVessel vessel)
    {
        orig(self, vessel);

        if (!Options.ShortcutNoticeCreatures.Value)
        {
            return;
        }

        if (vessel.TryGetShortcut(out IShortcut curShortcut))
        {
            foreach (ShortcutHandler.ShortCutVessel otherVessel in GetAllShortCutVessels(self))
            {
                if (otherVessel.TryGetShortcut(out IShortcut otherShortcut) && curShortcut.IsOppositeDirection(otherShortcut))
                {
                    Log.LogInfo($"{vessel.creature} and {otherVessel.creature} met in shortcut");

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
}
