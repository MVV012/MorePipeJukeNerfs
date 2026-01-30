using System;
using System.Collections.Generic;
using System.Linq;


namespace NoMorePipeJuking;


internal class ShortcutCreatureTracking
{
    internal class ShortcutCreaturePair
    {
        public AbstractCreature CreatureA { get; }
        public AbstractCreature CreatureB { get; }

        public AbstractCreature? ExitedCreature { get; set; }

        public ShortcutCreaturePair(AbstractCreature creatureA, AbstractCreature creatureB)
        {
            if (creatureA == creatureB)
            {
                throw new ArgumentException("Same creature is given twice to ShortcutCreaturePair");
            }

            CreatureA = creatureA;
            CreatureB = creatureB;
            ExitedCreature = null;
        }

        public AbstractCreature GetOtherCreature(AbstractCreature creature)
        {
            if (CreatureA == creature)
            {
                return CreatureB;
            }
            else
            {
                return CreatureA;
            }
        }

        public bool Contains(AbstractCreature creature)
        {
            return CreatureA == creature || CreatureB == creature;
        }
    }

    // There will be not much pairs, so it's okay to store them this way
    private static List<ShortcutCreaturePair> s_creaturePairs = [];


    public static void ApplyHooks()
    {
        On.ShortcutHandler.Update += ShortcutHandler_Update;
        On.ShortcutHandler.SpitOutCreature += ShortcutHandler_SpitOutCreature;
    }

    public static void RemoveHooks()
    {
        On.ShortcutHandler.Update -= ShortcutHandler_Update;
        On.ShortcutHandler.SpitOutCreature -= ShortcutHandler_SpitOutCreature;
    }

    private static void ShortcutHandler_Update(On.ShortcutHandler.orig_Update orig, ShortcutHandler self)
    {
        orig(self);

        // These loops and check were taken from Coder23848's mod Pipe Juke Nerf and modified
        for (int i = 0; i < self.transportVessels.Count; i++)
        {
            for (int j = i + 1; j < self.transportVessels.Count; j++)
            {
                ShortcutHandler.ShortCutVessel vesselA = self.transportVessels[i];
                ShortcutHandler.ShortCutVessel vesselB = self.transportVessels[j];

                if (vesselA.room == vesselB.room && (Math.Abs(vesselA.pos.x - vesselB.pos.x) + Math.Abs(vesselA.pos.y - vesselB.pos.y) <= 1))
                {
                    // Creatures are next to each other in shortcut
                    AddCreaturePair(vesselA.creature.abstractCreature, vesselB.creature.abstractCreature);
                }
            }
        }
    }

    private static void AddCreaturePair(AbstractCreature creatureA, AbstractCreature creatureB)
    {
        if (!s_creaturePairs.Any(pair => pair.Contains(creatureA) && pair.Contains(creatureB)))
        {
            s_creaturePairs.Add(new ShortcutCreaturePair(creatureA, creatureB));
            Plugin.Logger.LogDebug($"Added pair: {creatureA}, {creatureB}");
        }
    }

    private static void ShortcutHandler_SpitOutCreature(On.ShortcutHandler.orig_SpitOutCreature orig, ShortcutHandler self, ShortcutHandler.ShortCutVessel vessel)
    {
        orig(self, vessel);

        AbstractCreature cur = vessel.creature.abstractCreature;
        Tracker.CreatureRepresentation rep;

        // Iterating backwards to be able to remove elements
        for (int i = s_creaturePairs.Count - 1; i >= 0; i--)
        {
            var pair = s_creaturePairs[i];
            if (!pair.Contains(cur))
            {
                continue;
            }
            AbstractCreature other = pair.GetOtherCreature(cur);

            if (pair.ExitedCreature is null)
            {
                // First creature exited
                Plugin.Logger.LogDebug($"First exited: {cur} (other: {other})");

                if (CreatureRepresentationUtils.TryGetRepresentation(cur, other, out rep))
                {
                    rep.MakeVisibleOnShortCutEnd();
                    rep.MoveAwayFromShortcut();
                }
                if (CreatureRepresentationUtils.TryGetRepresentation(other, cur, out rep))
                {
                    rep.MakeImmediatelyVisible();
                    rep.MoveAwayFromShortcut();
                }
                pair.ExitedCreature = cur;
            }
            else if (pair.ExitedCreature != cur)
            {
                // Second creature exited
                Plugin.Logger.LogDebug($"Second exited: {cur} (other: {other})");

                if (CreatureRepresentationUtils.TryGetRepresentation(other, cur, out rep))
                {
                    if (rep is Tracker.ElaborateCreatureRepresentation elabRep)
                    {
                        elabRep.UnpauseStoppedGhost();
                    }
                }
                s_creaturePairs.RemoveAt(i);
            }
        }
    }
}
