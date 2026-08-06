global using static MorePipeJukeNerfs.LogWrapper;

using BepInEx;
using BepInEx.Logging;
using System.Diagnostics;
using System.Security.Permissions;
using MorePipeJukeNerfs.Shortcuts;
using MorePipeJukeNerfs.Remix;

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

    public void OnEnable()
    {
        Log = base.Logger;
        DebugLogInfo("OnEnable");

        On.RainWorld.OnModsInit += RainWorld_OnModsInit;

        VesselShortcutCWT.ApplyHooks();
        ShortcutPairTracking.ApplyHooks();
        NoticeCreatureUtils.ApplyHooks();
        PipeJukeNotifier.ApplyHooks();
        PlayerShortcutTrackerCWT.ApplyHooks();
        ShortcutCounterReducer.ApplyHooks();
        RemixUtils.ApplyHooks();

#if DEBUG
        // For Rain Reloader, does nothing without it
        MachineConnector.SetRegisteredOI(GUID, Options.Instance);
        MachineConnector.ReloadConfig(Options.Instance);
#endif
    }

    public void OnDisable()
    {
        On.RainWorld.OnModsInit -= RainWorld_OnModsInit;

        VesselShortcutCWT.RemoveHooks();
        ShortcutPairTracking.RemoveHooks();
        NoticeCreatureUtils.RemoveHooks();
        PipeJukeNotifier.RemoveHooks();
        PlayerShortcutTrackerCWT.RemoveHooks();
        ShortcutCounterReducer.RemoveHooks();
        RemixUtils.RemoveHooks();
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        if (_isInit) return;
        _isInit = true;

        MachineConnector.SetRegisteredOI(GUID, Options.Instance);
    }
}