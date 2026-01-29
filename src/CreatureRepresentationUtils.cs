using System;
using System.Collections.Generic;
using UnityEngine;

namespace NoMorePipeJuking;


public static class CreatureRepresentationUtils
{
    public static IEnumerable<Tracker.CreatureRepresentation> GetAllRepresentations(AbstractCreature tracking, AbstractCreature tracked)
    {
        // TODO: add other trackers support
        Tracker.CreatureRepresentation? rep = tracking?.abstractAI?.RealAI?.tracker.RepresentationForCreature(tracked, addIfMissing: false);
        return rep is not null ? [rep] : [];
    }

    /// <summary>
    /// Makes represented creature visible immediately without making it visible after that
    /// </summary>
    public static void MakeImmediatelyVisible(this Tracker.CreatureRepresentation rep)
    {
        if (rep is Tracker.ElaborateCreatureRepresentation elabRep)
        {
            ResetGhosts(elabRep);
        }
        else
        {
            rep.lastSeenCoord = rep.representedCreature.pos;
        }
    }

    /// <summary>
    /// Makes represented creature visible on shortcut's end.
    /// Should be called only when creature is in shortcut.
    /// UnpauseStoppedGhost should be called later for ElaborateCreatureRepresentation.
    /// </summary>
    public static void MakeVisibleOnShortCutEnd(this Tracker.CreatureRepresentation rep)
    {
        if (!rep.representedCreature.realizedCreature.inShortcut)
        {
            Plugin.Logger.LogWarning("MakeVisibleOnShortCutEnd was called for creature not in shortcut");
            Plugin.Logger.LogWarning($"Creature: {rep.representedCreature}");
            return;
        }

        if (rep is Tracker.ElaborateCreatureRepresentation elabRep)
        {
            PlaceStoppedGhostOnShortCutEnd(elabRep);
        }
        else
        {
            rep.lastSeenCoord = rep.representedCreature.pos;
        }
    }

    /// <summary>
    /// Resets representation's ghost position to current creature position
    /// </summary>
    public static void ResetGhosts(this Tracker.ElaborateCreatureRepresentation rep)
    {
        // This piece of code is copied from Tracker.ElaborateCreatureRepresentation.Update
        if (rep.ghosts.Count > 1)
        {
            rep.ghosts.RemoveRange(1, rep.ghosts.Count - 1);
            rep.bestGhost = rep.ghosts[0];
            rep.bestGhostDirty = false;
        }
        rep.ghosts[0].Reset();
        rep.lastSeenCoord = rep.ghosts[0].coord;
    }

    /// <summary>
    /// Sets representation's ghost position to shortcut end and stops it.
    /// Should be called only when creature is in shortcut.
    /// </summary>
    public static void PlaceStoppedGhostOnShortCutEnd(this Tracker.ElaborateCreatureRepresentation rep)
    {
        // This sets Ghost.coord to AbstractCreature.pos which is set to shortcut end by Creature.SuckedIntoShortCut
        ResetGhosts(rep);

        Tracker.Ghost ghost = rep.ghosts[0];

        // Ghost.pos is set to oldest position of mainBodyChunk, which is shortcut start, change it to shortcut end
        ghost.pos = rep.representedCreature.Room.realizedRoom.MiddleOfTile(ghost.coord);

        ghost.vel = Vector2.zero; // TODO: set velocity to face out of shortcut?
        ghost.stopped = true;
    }

    /// <summary>
    /// Unpauses first stopped ghost.
    /// Should be used after PlaceStoppedGhostOnShortCutEnd.
    /// </summary>
    public static void UnpauseStoppedGhost(this Tracker.ElaborateCreatureRepresentation rep)
    {
        Tracker.Ghost ghost = rep.ghosts[0];

        ghost.vel = rep.representedCreature.realizedCreature.mainBodyChunk.vel; // TODO: is it needed?
        ghost.stopped = false;
    }
}
