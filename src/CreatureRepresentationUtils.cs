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
        /// <param name="pause">Should ghost be paused</param>
        public void MoveToShortcutEntrance(AbstractRoom room, WorldCoordinate coord, bool pause = false)
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
                ghost.lastCoord = ghost.coord;
                if (pause)
                {
                    ghost.Pause();
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
        /// Unpauses unpushable ghosts
        /// Should be called after using MoveToShortcutEntrance
        /// </summary>
        public void UnpauseUnpushableGhosts()
        {
            if (rep is Tracker.ElaborateCreatureRepresentation elabRep)
            {
                foreach (Tracker.Ghost ghost in elabRep.ghosts)
                {
                    if (!ghost.Pushable)
                    {
                        ghost.Unpause();
                    }
                }
            }
        }
    }
}
