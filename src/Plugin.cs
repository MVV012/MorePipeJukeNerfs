global using static MorePipeJukeNerfs.LogWrapper;
using BepInEx;
using BepInEx.Logging;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using MorePipeJukeNerfs.Remix;
using MorePipeJukeNerfs.Shortcuts;
using System.Diagnostics;
using System.Reflection;
using System.Security.Permissions;

// Allows access to private members
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace MorePipeJukeNerfs;

internal static class LogWrapper
{
    public static ManualLogSource Log = null!;

    [Conditional("DEBUG")]
    public static void DebugLog(LogLevel level, object data)
    {
        Log.Log(level, data);
    }

    [Conditional("DEBUG")]
    public static void DebugLogInfo(object data)
    {
        Log.LogInfo(data);
    }
}

[BepInPlugin(GUID, NAME, VERSION)]
sealed public class Plugin : BaseUnityPlugin
{
    public const string GUID = "mvv012.morepipejukenerfs";
    public const string NAME = "More Pipe Juke Nerfs";
    public const string VERSION = "0.4.1";

    private bool _isInit = false;
    internal static bool DebugDependenciesEnabled = false;

    internal static List<Hook> ManualHooks = [];

    public void OnEnable()
    {
        Log = base.Logger;
        DebugLogInfo("OnEnable");

        On.RainWorld.OnModsInit += RainWorld_OnModsInit;

        VesselShortcutCWT.ApplyHooks();
        ShortcutPairTracking.ApplyHooks();
        NoticeCreatureUtils.ApplyHooks();
        GhostUtils.ApplyHooks();
        PlayerShortcutTrackerCWT.ApplyHooks();
        ShortcutCounterReducer.ApplyHooks();
        RemixUtils.ApplyHooks();

        PipeJukeNotifier.OnEnable();

#if DEBUG
        // For Rain Reloader, does nothing without it
        MachineConnector.SetRegisteredOI(GUID, Options.Instance);
        MachineConnector.ReloadConfig(Options.Instance);

        List<string> debugDeps = ["rwimgui", "fluffball.logmanager", "maxi-mol.mousedrag", "slime-cubed.devconsole", "warp"];
        List<string> disabledDeps = debugDeps.Where(dep => !ModManager.ActiveMods.Exists(mod => mod.id == dep)).ToList();
        DebugDependenciesEnabled = disabledDeps.Count == 0;
        if (DebugDependenciesEnabled)
        {
            Debug.DebugImGUIWindow.OnEnable();
        }
        else
        {
            DebugLog(LogLevel.Warning, $"Dependencies ({string.Join(", ", disabledDeps)}) for debug imgui menu are not enabled");
        }
#endif
    }

    public void OnDisable()
    {
        HookEndpointManager.RemoveAllOwnedBy(typeof(Plugin).Assembly);
        foreach (Hook hook in ManualHooks)
        {
            hook.Dispose();
        }
        ManualHooks.Clear();

        PipeJukeNotifier.OnDisable();

#if DEBUG
        if (DebugDependenciesEnabled)
        {
            Debug.DebugImGUIWindow.OnDisable();
        }
#endif
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        if (_isInit) return;
        _isInit = true;

        MachineConnector.SetRegisteredOI(GUID, Options.Instance);
    }
}