using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;


namespace NoMorePipeJuking;


internal class ShortcutCreatureTracking
{
    internal class ShortcutCreaturePair
    {
        public AbstractCreature CreatureA { get; }
        public AbstractCreature CreatureB { get; }

        public AbstractCreature? ExitedCreature { get; set; }
        public bool GhostNeedsToBeUnpaused { get; set; }

        public ShortcutCreaturePair(AbstractCreature creatureA, AbstractCreature creatureB)
        {
            if (creatureA == creatureB)
            {
                throw new ArgumentException("Same creature is given twice to ShortcutCreaturePair");
            }

            CreatureA = creatureA;
            CreatureB = creatureB;
            ExitedCreature = null;
            GhostNeedsToBeUnpaused = false;
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

    private static readonly CreatureTemplate.Type[] s_excludedCreatures = [CreatureTemplate.Type.Fly, CreatureTemplate.Type.Spider];


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
        if (!s_creaturePairs.Any(pair => pair.Contains(creatureA) && pair.Contains(creatureB))
            && !s_excludedCreatures.Contains(creatureA.creatureTemplate.type) 
            && !s_excludedCreatures.Contains(creatureB.creatureTemplate.type))
        {
            Plugin.Logger.LogDebug($"Added pair: {creatureA}, {creatureB}");
            s_creaturePairs.Add(new ShortcutCreaturePair(creatureA, creatureB));
        }
    }

    private static void ShortcutHandler_SpitOutCreature(On.ShortcutHandler.orig_SpitOutCreature orig, ShortcutHandler self, ShortcutHandler.ShortCutVessel vessel)
    {
        orig(self, vessel);

        AbstractCreature cur = vessel.creature.abstractCreature;

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
                FirstExitedShortcut(cur, other, pair);
                pair.ExitedCreature = cur;
            }
            else if (pair.ExitedCreature != cur)
            {
                // Second creature exited
                SecondExitedShortcut(cur, other, pair);
                s_creaturePairs.RemoveAt(i);
            }
        }
    }

    private static void FirstExitedShortcut(AbstractCreature cur, AbstractCreature other, ShortcutCreaturePair pair)
    {
        // TODO: move to plugin options
        bool unawareCreaturesNoticeEachOther = true;
        bool awareCreaturesNoticeEachOther = true;

        Plugin.Logger.LogDebug($"First exited: {cur} (other: {other})");

        Tracker.CreatureRepresentation? rep;

        try
        {
            if (cur.TryGetRepresentation(other, out rep))
            {
                if (awareCreaturesNoticeEachOther)
                {
                    rep.UpdateStateAndRelationship();
                    rep.MakeVisibleOnShortCutEnd();
                    rep.MoveAwayFromShortcut();
                    pair.GhostNeedsToBeUnpaused = true;
                }
            }
            else
            {
                if (unawareCreaturesNoticeEachOther)
                {
                    cur.NoticeCreature(other);
                    if (cur.TryGetRepresentation(other, out rep))
                    {
                        rep.MakeVisibleOnShortCutEnd();
                        rep.MoveAwayFromShortcut();
                        pair.GhostNeedsToBeUnpaused = true;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Failed to make {cur} aware of {other} position");
            Plugin.Logger.LogError($"Exception: {e}");
        }


        try {
            if (other.TryGetRepresentation(cur, out rep))
            {
                if (awareCreaturesNoticeEachOther)
                {
                    rep.UpdateStateAndRelationship();
                    rep.MakeImmediatelyVisible();
                    rep.MoveAwayFromShortcut();
                }
            }
            else
            {
                if (unawareCreaturesNoticeEachOther)
                {
                    other.NoticeCreature(cur);
                    rep = other.GetRepresentation(cur);

                    rep?.MakeImmediatelyVisible();
                    rep?.MoveAwayFromShortcut();
                }
            }
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Failed to make {other} aware of {cur} position");
            Plugin.Logger.LogError($"Exception: {e}");
        }
    }

    private static void SecondExitedShortcut(AbstractCreature cur, AbstractCreature other, ShortcutCreaturePair pair)
    {
        Plugin.Logger.LogDebug($"Second exited: {cur} (other: {other})");

        try
        {
            if (pair.GhostNeedsToBeUnpaused && other.GetRepresentation(cur) is Tracker.ElaborateCreatureRepresentation elabRep)
            {
                elabRep.UnpauseStoppedGhost();
            }
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Failed to unpause {other} ghost for {cur}");
            Plugin.Logger.LogError($"Exception: {e}");
        }
    }
}
