using RWCustom;
using System.Reflection;
using UnityEngine;

namespace MorePipeJukeNerfs.Debug;

internal static class RWGameUtils
{
    public static bool TryGetRWGame(out RainWorldGame game) {
        if (Custom.rainWorld.processManagerInitialized && Custom.rainWorld.processManager.currentMainLoop is RainWorldGame rwgame)
        {
            game = rwgame;
            return true;
        }
        else
        {
            game = null!;
            return false;
        }
    }

    private static bool s_checkedForDebugVis = false;
    private static MethodInfo? s_debugVisClearAll = null;

    extension(RainWorldGame game)
    {
        // Stolen from RWIMGUI docs
        public bool TryGetAbstractCreatureById(int id, out AbstractCreature creature)
        {
            foreach (var absRoom in game.world.abstractRooms)
            {
                foreach (var absCreature in absRoom.creatures)
                {
                    if (absCreature.ID.number == id)
                    {
                        creature = absCreature;
                        return true;
                    }
                }
            }
            creature = null!;
            return false;
        }

        public AbstractCreature SpawnCreature(WorldCoordinate coord, CreatureTemplate.Type creatureType)
        {
            AbstractRoom room = game.world.GetAbstractRoom(coord.room);
            AbstractCreature creature = new AbstractCreature(game.world, StaticWorld.GetCreatureTemplate(creatureType), null, coord, game.GetNewID());
            room.AddEntity(creature);
            if (room.realizedRoom != null)
            {
                creature.RealizeInRoom();
            }
            return creature;
        }

        public void MurderEveryone()
        {
            MouseDrag.Destroy.DestroyRegionObjects(game, creatures: true, items: true);
            game.shortcuts.transportVessels = game.shortcuts.transportVessels.Where(vessel => vessel.creature is Player { isNPC: false }).ToList();
            game.shortcuts.borderTravelVessels = game.shortcuts.borderTravelVessels.Where(vessel => vessel.creature is Player { isNPC: false }).ToList();
            game.shortcuts.betweenRoomsWaitingLobby = game.shortcuts.betweenRoomsWaitingLobby.Where(vessel => vessel.creature is Player { isNPC: false }).ToList();

            if (!s_checkedForDebugVis)
            {
                s_checkedForDebugVis = true;
                if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name.Equals("DebugVisualizer", StringComparison.OrdinalIgnoreCase)))
                {
                    s_debugVisClearAll = Type.GetType("DebugVisualizer.DebugVisualizer, DebugVisualizer", throwOnError: false)?.GetMethod("ClearAllSprites", BindingFlags.Instance | BindingFlags.NonPublic);
                }
            }
            if (s_debugVisClearAll != null)
            {
                game.ClearDebugVisualizerSprites();
            }
        }

        private void ClearDebugVisualizerSprites()
        {
            s_debugVisClearAll!.Invoke(DebugVisualizer.DebugVisualizer.Instance, [null]);
        }

        public void Update(int times)
        {
            for (int i = 0; i < times; i++)
            {
                game.Update();
            }
        }

        public void UpdateWhile(Func<bool> pred)
        {
            while (pred())
            {
                game.Update();
            }
        }
    }

    extension(IntVector2 tile)
    {
        public Vector2 MiddleOfTile => new Vector2(10f + (float)tile.x * 20f, 10f + (float)tile.y * 20f);
    }

    extension(WorldCoordinate wc)
    {
        public Vector2 MiddleOfTile => wc.Tile.MiddleOfTile;
    }

    extension(Vector2 pos)
    {
        public IntVector2 TilePosition => new IntVector2((int)((pos.x + 20f) / 20f) - 1, (int)((pos.y + 20f) / 20f) - 1);
    }
}
