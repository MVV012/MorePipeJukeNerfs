using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace MorePipeJukeNerfs.Debug.Tests;

internal static class PlayerGraphicsInitiateSpritesFix
{
    public static void ApplyHooks()
    {
        IL.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
    }

    private static void PlayerGraphics_InitiateSprites(ILContext il)
    {
        ILCursor c = new(il);

        c.GotoNext(MoveType.After,
            x => x.MatchBrtrue(out _)
        );
        ILLabel label = c.MarkLabel();

        ILCursor d = new(il);

        d.GotoNext(MoveType.Before,
            x => x.MatchLdarg(0)
        );
        d.Emit(OpCodes.Br, label);
    }
}
