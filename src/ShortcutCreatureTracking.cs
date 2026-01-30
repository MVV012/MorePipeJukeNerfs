using System;
using System.Collections.Generic;
using System.Linq;


namespace NoMorePipeJuking;


internal class ShortcutCreatureTracking
{
    internal class ShortcutCreaturePair
    {
        public Creature CreatureA { get; }
        public Creature CreatureB { get; }

        public Creature? ExitedCreature { get; set; }

        public ShortcutCreaturePair(Creature creatureA, Creature creatureB)
        {
            if (creatureA == creatureB)
            {
                throw new ArgumentException("Same creature is given twice to ShortcutCreaturePair");
            }

            CreatureA = creatureA;
            CreatureB = creatureB;
            ExitedCreature = null;
        }

        public Creature GetOtherCreature(Creature creature)
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

        public bool Contains(Creature creature)
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

        for (int i = 0; i < self.transportVessels.Count; i++)
        {
            for (int j = i + 1; j < self.transportVessels.Count; j++)
            {
                ShortcutHandler.ShortCutVessel vesselA = self.transportVessels[i];
                ShortcutHandler.ShortCutVessel vesselB = self.transportVessels[j];

                // This check was taken from Coder23848's mod Pipe Juke Nerf
                if (vesselA.room == vesselB.room && vesselA.pos.FloatDist(vesselB.pos) <= 1f)
                {
                    // Creatures are next to each other in shortcut
                    AddCreaturePair(vesselA.creature, vesselB.creature);
                }
            }
        }
    }

    private static void AddCreaturePair(Creature creatureA, Creature creatureB)
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

        Creature cur = vessel.creature;
        Tracker.CreatureRepresentation rep;

        // Iterating backwards to be able to remove elements
        for (int i = s_creaturePairs.Count - 1; i >= 0; i--)
        {
            var pair = s_creaturePairs[i];
            if (!pair.Contains(cur))
            {
                continue;
            }
            Creature other = pair.GetOtherCreature(cur);

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
