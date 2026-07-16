using MonoMod.RuntimeDetour;

namespace MorePipeJukeNerfs;

public static class NoticeCreatureUtils
{
    private static bool s_overrideVisualContact = false;

    private static Hook? s_visualContactHook;

    public static void ApplyHooks()
    {
        s_visualContactHook = new Hook(
            typeof(Tracker.CreatureRepresentation).GetProperty(nameof(Tracker.CreatureRepresentation.VisualContact)).GetGetMethod(),
            CreatureRepresentation_VisualContact
        );
    }

    public static void RemoveHooks()
    {
        s_visualContactHook?.Undo();
        s_visualContactHook?.Dispose();
        s_visualContactHook = null;
    }

    private static bool CreatureRepresentation_VisualContact(Func<Tracker.CreatureRepresentation, bool> orig, Tracker.CreatureRepresentation self)
    {
        bool res = orig(self);
        if (s_overrideVisualContact)
        {
            return true;
        }
        return res;
    }

    /// <summary>
    /// Updates tracked state and relationship of creature representation.
    /// </summary>
    public static void UpdateStateAndRelationship(this Tracker.CreatureRepresentation rep)
    {
        try
        {
#if DEBUG
            CreatureTemplate.Relationship? oldRel = rep.dynamicRelationship?.currentRelationship;
#endif

            // It's okay if game is single-threaded (Not okay otherwise)
            s_overrideVisualContact = true;
            rep.dynamicRelationship?.Update();

#if DEBUG
            CreatureTemplate.Relationship? newRel = rep.dynamicRelationship?.currentRelationship;
            if (newRel != null && !newRel.Equals(oldRel))
            {
                DebugLogInfo($"New relationship of {rep.parent.AI.creature} to {rep.representedCreature}: {(oldRel == null ? "None" : oldRel)} -> {rep.dynamicRelationship?.currentRelationship}");
            }
#endif
        }
        catch (Exception e)
        {
            Log.LogError($"Failed to update relationship of {rep.parent.AI.creature} towards {rep.representedCreature}");
            Log.LogError($"Exception: {e}");
        }
        finally
        {
            s_overrideVisualContact = false;
        }
    }

    /// <summary>
    /// Makes creature notice another creature.
    /// Should not be called if creature is already tracked.
    /// </summary>
    public static void NoticeCreature(this AbstractCreature tracking, AbstractCreature tracked)
    {
        try
        {
            var rep = tracking.abstractAI?.RealAI?.tracker?.CreatureNoticed(tracked);
            rep?.visualContact = false;
            rep?.UpdateStateAndRelationship();
        }
        catch (Exception e)
        {
            Log.LogError($"Failed to make {tracking} notice {tracked}");
            Log.LogError($"Exception: {e}");
        }
    }
}
