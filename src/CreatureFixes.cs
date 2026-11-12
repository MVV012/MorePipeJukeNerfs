using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RWCustom;
using UnityEngine;

namespace MorePipeJukeNerfs;

public static class CreatureFixes
{
    public static bool PipeJuking { get; set; } = false;

    public static void ApplyHooks()
    {
        On.LizardGraphics.CreatureInterestBonus += LizardGraphics_CreatureInterestBonus;
        Plugin.ManualHooks.Add(new Hook(
            typeof(Scavenger.AttentiveAnimation).GetProperty(nameof(Scavenger.AttentiveAnimation.LookPoint)).GetMethod,
            Scavenger_AttentiveAnimations_get_LookPoint
        ));
        On.CentipedeAI.OverChasm += CentipedeAI_OverChasm;
        On.NeedleWormAI.UncomfortableToAfraidRelationshipModifier += NeedleWormAI_UncomfortableToAfraidRelationshipModifier;
        IL.MouseAI.IUseARelationshipTracker_UpdateDynamicRelationship += ILHookUtils.Wrap(MouseAI_IUseARelationshipTracker_UpdateDynamicRelationship_ILHook);
        IL.MoreSlugcats.YeekAI.IUseARelationshipTracker_UpdateDynamicRelationship += ILHookUtils.Wrap(YeekAI_IUseARelationshipTracker_UpdateDynamicRelationship_ILHook);
        IL.Watcher.BigMothAI.IUseARelationshipTracker_UpdateDynamicRelationship += ILHookUtils.Wrap(BigMothAI_IUseARelationshipTracker_UpdateDynamicRelationship_ILHook);
        IL.JetFishAI.IUseARelationshipTracker_UpdateDynamicRelationship += ILHookUtils.Wrap(JetFishAI_IUseARelationshipTracker_UpdateDynamicRelationship_ILHook);
        IL.Watcher.AnglerAI.IUseARelationshipTracker_UpdateDynamicRelationship += ILHookUtils.Wrap(AnglerAI_IUseARelationshipTracker_UpdateDynamicRelationship_ILHook);
        IL.DaddyAI.Update += ILHookUtils.Wrap(DaddyAI_Update_ILHook); // Probably game bug?
        IL.DropBugAI.IUseARelationshipTracker_UpdateDynamicRelationship += ILHookUtils.Wrap(DropBugAI_IUseARelationshipTracker_UpdateDynamicRelationship_ILHook); // Actual game bug
        IL.EggBugAI.CreatureSpotted += ILHookUtils.Wrap(EggBugAI_CreatureSpotted_ILHook);
    }

    private static float LizardGraphics_CreatureInterestBonus(On.LizardGraphics.orig_CreatureInterestBonus orig, LizardGraphics self, Tracker.CreatureRepresentation crit, float score)
    {
        if (!PipeJuking) return orig(self, crit, score);

        if (self.lizard.room == null || self.lizard.room.abstractRoom.index != crit.representedCreature.Room.index)
        {
            return score * 1f; // Bonus is returned as if creature is in front of lizard
        }

        return orig(self, crit, score);
    }

    private static Vector2 Scavenger_AttentiveAnimations_get_LookPoint(Func<Scavenger.AttentiveAnimation, Vector2> orig, Scavenger.AttentiveAnimation self)
    {
        if (!PipeJuking) return orig(self);

        if (self.scavenger.room == null)
        {
            return self.point;
        }

        return orig(self);
    }

    private static float CentipedeAI_OverChasm(On.CentipedeAI.orig_OverChasm orig, CentipedeAI self, RWCustom.IntVector2 testPos)
    {
        if (!PipeJuking) return orig(self, testPos);

        if (self.centipede.room == null)
        {
            return 0f; // No risk of falling to death pit
        }

        return orig(self, testPos);
    }

    private static CreatureTemplate.Relationship NeedleWormAI_UncomfortableToAfraidRelationshipModifier(On.NeedleWormAI.orig_UncomfortableToAfraidRelationshipModifier orig, NeedleWormAI self, RelationshipTracker.DynamicRelationship dRel, CreatureTemplate.Relationship currRel)
    {
        if (!PipeJuking) return orig(self, dRel, currRel);

        if (self.worm.room?.aimap == null)
        {
            return currRel;
        }

        return orig(self, dRel, currRel);
    }

    private static void MouseAI_IUseARelationshipTracker_UpdateDynamicRelationship_ILHook(ILContext il)
    {
        ILCursor c = new(il);
        c.GotoNext(x => x.MatchLdfld<CreatureTemplate>(nameof(CreatureTemplate.canFly)));

        ILCursor d = new(c);
        ILLabel label = null!;
        d.GotoNext(x => x.MatchBrtrue(out label));

        c.GotoPrev(x => x.MatchLdfld<MouseAI>(nameof(MouseAI.mouse)));
        c.GotoPrev(x => x.MatchLdfld<RelationshipTracker.DynamicRelationship>(nameof(RelationshipTracker.DynamicRelationship.trackerRep)));
        c.GotoPrev(MoveType.Before, x => x.MatchLdarg(1));
        c.MoveAfterLabels();

        c.Emit(OpCodes.Ldarg_0);
        c.EmitStaticDelegate(SkipBranch);
        c.Emit(OpCodes.Brtrue, label);

        static bool SkipBranch(MouseAI ai)
        {
            if (!PipeJuking) return false;

            return ai.mouse.room == null;
        }
    }

    private static void YeekAI_IUseARelationshipTracker_UpdateDynamicRelationship_ILHook(ILContext il)
    {
        ILCursor c = new(il);
        c.GotoNext(x => x.MatchLdfld<CreatureTemplate>(nameof(CreatureTemplate.canFly)));

        ILCursor d = new(c);
        ILLabel label = null!;
        d.GotoNext(x => x.MatchBrtrue(out label));

        c.GotoPrev(x => x.MatchLdfld<MoreSlugcats.YeekAI>(nameof(MoreSlugcats.YeekAI.yeek)));
        c.GotoPrev(x => x.MatchLdfld<RelationshipTracker.DynamicRelationship>(nameof(RelationshipTracker.DynamicRelationship.trackerRep)));
        c.GotoPrev(MoveType.Before, x => x.MatchLdarg(1));
        c.MoveAfterLabels();

        c.Emit(OpCodes.Ldarg_0);
        c.EmitStaticDelegate(SkipBranch);
        c.Emit(OpCodes.Brtrue, label);

        static bool SkipBranch(MoreSlugcats.YeekAI ai)
        {
            if (!PipeJuking) return false;

            return ai.yeek.room == null;
        }
    }

    private static void BigMothAI_IUseARelationshipTracker_UpdateDynamicRelationship_ILHook(ILContext il)
    {
        ILCursor c = new(il);
        c.GotoNext(x => x.MatchCallvirt<AbstractRoom>(nameof(AbstractRoom.AttractionForCreature)));

        c.GotoPrev(MoveType.After,
            x => x.MatchLdfld<Watcher.BigMothAI>(nameof(Watcher.BigMothAI.bug))
        );

        ILCursor d = new(c);
        d.GotoNext(MoveType.After,
            x => x.MatchGetter<Room>(nameof(Room.abstractRoom))
        );
        ILLabel label = d.MarkLabel();

        c.EmitStaticDelegate(GetAbstractRoom);
        c.Emit(OpCodes.Br_S, label);

        static AbstractRoom GetAbstractRoom(Watcher.BigMoth smolMof)
        {
            if (!PipeJuking) return smolMof.room.abstractRoom;

            return smolMof.abstractCreature.Room;
        }
    }

    private static void JetFishAI_IUseARelationshipTracker_UpdateDynamicRelationship_ILHook(ILContext il)
    {
        ILCursor c = new(il);
        c.GotoNext(x => x.MatchLdsfld<CreatureTemplate.Relationship.Type>(nameof(CreatureTemplate.Relationship.Type.SocialDependent)));
        c.GotoNext(x => x.MatchLdfld<Room>(nameof(Room.defaultWaterLevel)));

        c.GotoPrev(x => x.MatchLdsfld<CreatureTemplate.Relationship.Type>(nameof(CreatureTemplate.Relationship.Type.Antagonizes)));

        ILLabel label = null!;
        c.GotoNext(MoveType.After, x => x.MatchBrfalse(out label));

        c.Emit(OpCodes.Ldarg_0);
        c.EmitStaticDelegate(SkipBranch);
        c.Emit(OpCodes.Brtrue, label);

        static bool SkipBranch(JetFishAI ai)
        {
            if (!PipeJuking) return false;

            return ai.fish.room == null;
        }
    }

    private static void AnglerAI_IUseARelationshipTracker_UpdateDynamicRelationship_ILHook(ILContext il)
    {
        ILCursor c = new(il);
        c.GotoNext(
            x => x.MatchLdcR4(1),
            x => x.MatchStfld<CreatureTemplate.Relationship>(nameof(CreatureTemplate.Relationship.intensity))
        );

        c.GotoNext(x => x.MatchStloc(6));
        c.GotoPrev(MoveType.Before, x => x.MatchLdloc(4));
        ILLabel label = c.MarkLabel();

        c.GotoPrev(
            x => x.MatchLdfld<UpdatableAndDeletable>(nameof(UpdatableAndDeletable.room)),
            x => x.MatchBneUn(out _)
        );

        c.GotoPrev(MoveType.Before,
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<Watcher.AnglerAI>(nameof(Watcher.AnglerAI.fish))
        );

        c.EmitStaticDelegate(SkipVisualContacCheck);
        c.Emit(OpCodes.Brtrue, label);

        static bool SkipVisualContacCheck()
        {
            if (!PipeJuking) return false;

            return true;
        }
    }

    private static void DaddyAI_Update_ILHook(ILContext il)
    {
        ILCursor c = new(il);

        c.GotoNext(x => x.MatchCallvirt<AImap>(nameof(AImap.TileAccessibleToCreature)));

        ILCursor d = new(c);
        ILLabel label = null!;
        d.GotoNext(x => x.MatchBrfalse(out label));

        // grabPath[0]
        c.GotoPrev(
            x => x.MatchLdfld<Tentacle>(nameof(Tentacle.grabPath)),
            x => x.MatchLdcI4(0),
            x => x.MatchCallvirt(typeof(List<RWCustom.IntVector2>).GenericName, "get_Item")
        );

        c.GotoPrev(MoveType.Before,
            x => x.MatchLdarg(0),
            x => x.MatchGetter<DaddyAI>(nameof(DaddyAI.daddy)),
            x => x.MatchLdfld<UpdatableAndDeletable>(nameof(UpdatableAndDeletable.room))
        );

        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldloc_S, (byte)9);
        c.EmitStaticDelegate(GrabPathEmpty);
        c.Emit(OpCodes.Brtrue, label);

        static bool GrabPathEmpty(DaddyAI ai, int j)
        {
            return ai.daddy.tentacles[j].grabPath.Count == 0;
        }
    }

    private static void DropBugAI_IUseARelationshipTracker_UpdateDynamicRelationship_ILHook(ILContext il)
    {
        ILCursor c = new(il);

        c.GotoNext(x => x.MatchLdsfld<Watcher.WatcherEnums.CreatureTemplateType>(nameof(Watcher.WatcherEnums.CreatureTemplateType.Barnacle)));

        c.GotoNext(MoveType.Before,
            x => x.MatchGetter<AbstractCreature>(nameof(AbstractCreature.realizedCreature)),
            x => x.MatchGetter<Creature>(nameof(Creature.dead))
        );
        c.MoveAfterLabels();

        ILCursor d = new(c);
        d.GotoNext(MoveType.After,
            x => x.MatchGetter<Creature>(nameof(Creature.dead))
        );
        ILLabel label = d.MarkLabel();

        c.EmitStaticDelegate(GetDead);
        c.Emit(OpCodes.Br_S, label);

        static bool GetDead(AbstractCreature barnacle)
        {
            if (barnacle.realizedCreature != null) return barnacle.realizedCreature.dead;

            return barnacle.state.dead;
        }
    }

    private static void EggBugAI_CreatureSpotted_ILHook(ILContext il)
    {
        ILCursor c = new(il);

        c.GotoNext(x => x.MatchLdsfld<CreatureTemplate.Relationship.Type>(nameof(CreatureTemplate.Relationship.Type.Afraid)));

        c.GotoPrev(MoveType.Before,
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<EggBugAI>(nameof(EggBugAI.bug)),
            x => x.MatchGetter<Creature>(nameof(Creature.safariControlled))
        );
        c.MoveAfterLabels();

        ILCursor d = new(c);
        ILLabel label = null!;
        d.GotoNext(x => x.MatchBrfalse(out label));

        c.EmitStaticDelegate(SkipTryJump);
        c.Emit(OpCodes.Brtrue, label);

        static bool SkipTryJump()
        {
            return PipeJuking;
        }

        c.GotoNext(x => x.MatchCallvirt<EggBug>(nameof(EggBug.Suprise)));

        ILLabel label2 = null!;
        c.GotoPrev(MoveType.After,
            x => x.MatchBleUn(out label2)
        );

        c.Emit(OpCodes.Ldarg_0);
        c.EmitStaticDelegate(ReplaceSurprise);
        c.Emit(OpCodes.Brtrue, label2);

        static bool ReplaceSurprise(EggBugAI ai)
        {
            if (!PipeJuking) return false;

            if (!ai.bug.Consious)
            {
                return true;
            }
            ai.bug.shake = Math.Max(ai.bug.shake, UnityEngine.Random.Range(5, 15));
            ai.fear = Custom.LerpAndTick(ai.fear, 1f, 0.3f, 1f / 7f);
            return true;
        }
    }
}
