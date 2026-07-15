using RWCustom;
using UnityEngine;

namespace MorePipeJukeNerfs;

public static class CreatureRepresentationUtils
{
    extension(AbstractCreature tracking)
    {
        public Tracker.CreatureRepresentation? GetRepresentation(AbstractCreature tracked)
        {
            return tracking.abstractAI?.RealAI?.tracker?.RepresentationForCreature(tracked, addIfMissing: false);
        }

        public bool TryGetRepresentation(AbstractCreature tracked, out Tracker.CreatureRepresentation rep)
        {
            rep = tracking.GetRepresentation(tracked)!;
            return rep is not null;
        }
    }


    extension(Tracker.CreatureRepresentation rep)
    {
        /// <summary>
        /// Moves representation to a tile in front of shortcut's entrance
        /// </summary>
        /// <param name="room">Abstract room of shortcut entrance</param>
        /// <param name="coord">World coordinate of shortcut entrance</param>
        /// <param name="stop">Should ghost be stopped</param>
        public void MoveToShortcutEntrance(AbstractRoom room, WorldCoordinate coord, bool stop = false)
        {
            if (rep is Tracker.ElaborateCreatureRepresentation elabRep)
            {
                if (elabRep.ghosts.Count > 1)
                {
                    elabRep.ghosts.RemoveRange(1, elabRep.ghosts.Count - 1);
                    elabRep.bestGhost = elabRep.ghosts[0];
                    elabRep.bestGhostDirty = false;
                }
                Tracker.Ghost ghost = elabRep.ghosts[0];
                ghost.generation = 0;
                ghost.stopped = stop;
                ghost.lastCoord = coord;
                if (room.realizedRoom != null)
                {
                    IntVector2 entraceHoleDirection = room.realizedRoom.ShorcutEntranceHoleDirection(coord.Tile);
                    ghost.coord = coord + entraceHoleDirection;
                    ghost.vel = entraceHoleDirection.ToVector2() * 2f;
                }
                else
                {
                    ghost.coord = coord;
                    ghost.vel = Vector2.zero;
                }
                // Copied from Room.MiddleOfTile (why there is no static variant?)
                ghost.pos = new Vector2(10f + (float)ghost.coord.x * 20f, 10f + (float)ghost.coord.y * 20f);

                rep.lastSeenCoord = ghost.coord;
            }
            else
            {
                if (room.realizedRoom != null)
                {
                    rep.lastSeenCoord = coord + room.realizedRoom.ShorcutEntranceHoleDirection(coord.Tile);
                }
                else
                {
                    rep.lastSeenCoord = coord;
                }
            }
            rep.ticksSinceSeen = 0;

            if (room.realizedRoom == null)
            {
                DebugLogInfo($"MoveToShortcutEntrance {coord}: Room {room.name} is NOT realized");
            }
            else if (!room.realizedRoom.shortCutsReady)
            {
                DebugLogInfo($"MoveToShortcutEntrance {coord}: Room {room.name} shortcuts are NOT ready");
            }
        }

        /// <summary>
        /// Unpauses stopped ghost
        /// Should be called after using MoveToShortcutEntrance
        /// </summary>
        public void UnpauseStoppedGhost()
        {
            if (rep is Tracker.ElaborateCreatureRepresentation elabRep)
            {
                if (elabRep.ghosts.Count > 1)
                {
                    Log.LogWarning($"UnpauseStoppedGhost: {elabRep.parent.AI.creature}'s representation of " +
                        $"{elabRep.representedCreature} has {elabRep.ghosts.Count} ghosts");
                    return;
                }

                elabRep.ghosts[0].stopped = false;
            }
        }
    }
}
