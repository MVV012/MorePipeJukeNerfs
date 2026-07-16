global using static MorePipeJukeNerfs.LogWrapper;

using BepInEx;
using BepInEx.Logging;
using MorePipeJukeNerfs.Shortcuts;
using System.Diagnostics;
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

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
sealed public class Plugin : BaseUnityPlugin
{
    public const string PLUGIN_GUID = "mvv012.morepipejukenerfs";
    public const string PLUGIN_NAME = "More Pipe Juke Nerfs";
    public const string PLUGIN_VERSION = "0.3.0";

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
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        if (_isInit) return;
        _isInit = true;

        // MachineConnector.SetRegisteredOI(PLUGIN_GUID, Options.Instance);
    }
}