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

        public bool FirstCreatureExited { get; set; }

        public ShortcutCreaturePair(Creature creatureA, Creature creatureB)
        {
            if (creatureA == creatureB)
            {
                throw new ArgumentException("Same creature is given twice to ShortcutCreaturePair");
            }

            CreatureA = creatureA;
            CreatureB = creatureB;
            FirstCreatureExited = false;
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
    }

    // There will be not much pairs, so it's okay to store them this way
    private static List<ShortcutCreaturePair> s_creaturePairs = [];


    public static void ApplyHooks()
    {
        On.ShortcutHandler.Update += ShortcutHandler_Update;
        On.Creature.SpitOutOfShortCut += Creature_SpitOutOfShortCut;
    }

    public static void RemoveHooks()
    {
        On.ShortcutHandler.Update -= ShortcutHandler_Update;
        On.Creature.SpitOutOfShortCut -= Creature_SpitOutOfShortCut;
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
        if (!s_creaturePairs.Any(pair =>
        {
            return pair.CreatureA == creatureA && pair.CreatureB == creatureB
                   || pair.CreatureA == creatureB && pair.CreatureB == creatureA;
        }))
        {
            s_creaturePairs.Add(new ShortcutCreaturePair(creatureA, creatureB));
            Plugin.Logger.LogDebug($"Added pair: {creatureA} {creatureB}");
        }
    }

    private static void Creature_SpitOutOfShortCut(On.Creature.orig_SpitOutOfShortCut orig, Creature self, RWCustom.IntVector2 pos, Room newRoom, bool spitOutAllSticks)
    {
        orig(self, pos, newRoom, spitOutAllSticks);

        // Iterating backwards to be able to remove elements
        for (int i = s_creaturePairs.Count - 1; i >= 0; i--)
        {
            var pair = s_creaturePairs[i];

            if (pair.CreatureA == self || pair.CreatureB == self)
            {
                Creature other = pair.GetOtherCreature(self);
                if (!pair.FirstCreatureExited)
                {
                    // First creature exited
                    Plugin.Logger.LogDebug($"First exited: {self} (other: {other})");

                    foreach (var rep in CreatureRepresentationUtils.GetAllRepresentations(self.abstractCreature, other.abstractCreature))
                    {
                        rep.MakeVisibleOnShortCutEnd();
                    }
                    foreach (var rep in CreatureRepresentationUtils.GetAllRepresentations(other.abstractCreature, self.abstractCreature))
                    {
                        rep.MakeImmediatelyVisible();
                        Plugin.Logger.LogDebug($"Ghost speed immediate: {(rep as Tracker.ElaborateCreatureRepresentation).ghosts[0].vel}");

                    }
                    pair.FirstCreatureExited = true; // TODO: Fix. This is bugged if first creature exits from other shortcut quickly
                }
                else
                {
                    // Second creature exited
                    Plugin.Logger.LogDebug($"Second exited: {self} (other: {other})");

                    /* foreach (var rep in CreatureRepresentationUtils.GetAllRepresentations(self.abstractCreature, other.abstractCreature))
                    {
                        // TODO: should ghost be pushed for time wasted in shortcut???
                    } */
                    foreach (var rep in CreatureRepresentationUtils.GetAllRepresentations(other.abstractCreature, self.abstractCreature))
                    {
                        if (rep is Tracker.ElaborateCreatureRepresentation elabRep)
                        {
                            elabRep.UnpauseStoppedGhost();
                            Plugin.Logger.LogDebug($"Ghost speed unpaused: {(rep as Tracker.ElaborateCreatureRepresentation).ghosts[0].vel}");

                            Plugin.Logger.LogDebug($"Unpaused ghost");

                        }
                    }
                    s_creaturePairs.RemoveAt(i);
                }
            }
        }
    }
}
