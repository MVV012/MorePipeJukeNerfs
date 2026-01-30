using MonoMod.RuntimeDetour;
using System;


namespace NoMorePipeJuking;


internal static class NoticeCreatureUtils
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
            // It's okay if game is single-threaded (Not okay otherwise)
            s_overrideVisualContact = true;
            rep?.dynamicRelationship?.Update();
            s_overrideVisualContact = false;

            Plugin.Logger.LogDebug($"New Rel: {rep?.representedCreature}, {rep?.dynamicRelationship?.currentRelationship}");
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Failed to update relationship of {rep.parent.AI.creature} towards {rep.representedCreature}");
            Plugin.Logger.LogError($"Exception: {e}");
        }
    }

    /// <summary>
    /// Makes creature notice another creature.
    /// Should not be called, if creature is already tracked.
    /// </summary>
    public static void NoticeCreature(this AbstractCreature tracking, AbstractCreature tracked)
    {
        try
        {
            var rep = tracking?.abstractAI?.RealAI?.tracker?.CreatureNoticed(tracked);
            rep?.visualContact = false;
            rep?.UpdateStateAndRelationship();
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Failed to make {tracking} notice {tracked}");
            Plugin.Logger.LogError($"Exception: {e}");
        }
    }
}
