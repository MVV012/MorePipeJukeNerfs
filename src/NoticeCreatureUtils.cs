using BepInEx.Logging;
using MonoMod.RuntimeDetour;

namespace MorePipeJukeNerfs;

public static class NoticeCreatureUtils
{
    private static bool s_overrideVisualContact = false;

    public static void ApplyHooks()
    {
        Plugin.ManualHooks.Add(new Hook(
            typeof(Tracker.CreatureRepresentation).GetProperty(nameof(Tracker.CreatureRepresentation.VisualContact)).GetGetMethod(),
            CreatureRepresentation_VisualContact
        ));
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
            ReleaseLog(LogLevel.Error, $"Failed to update relationship of {rep.parent.AI.creature} towards {rep.representedCreature}. Exception: {e}");
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
        }
        catch (Exception e)
        {
            ReleaseLog(LogLevel.Error, $"Failed to make {tracking} notice {tracked}. Exception: {e}");
        }
    }
}
