using RWCustom;

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
    }
}
