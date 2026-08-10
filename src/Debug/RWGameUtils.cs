using RWCustom;
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

        public AbstractCreature SpawnCreature(AbstractRoom room, IntVector2 tile, CreatureTemplate.Type creatureType)
        {
            WorldCoordinate coord = Custom.MakeWorldCoordinate(tile, room.index);
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
            game.shortcuts.transportVessels.Clear();
            game.shortcuts.borderTravelVessels.Clear();
            game.shortcuts.betweenRoomsWaitingLobby.Clear();
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

    extension(Vector2 pos)
    {
        public IntVector2 TilePosition => new IntVector2((int)((pos.x + 20f) / 20f) - 1, (int)((pos.y + 20f) / 20f) - 1);
    }
}
