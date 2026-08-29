using BepInEx;
using BepInEx.Logging;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using MorePipeJukeNerfs.Remix;
using MorePipeJukeNerfs.Shortcuts;
using System.Security.Permissions;

// Allows access to private members
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace MorePipeJukeNerfs;

[BepInPlugin(GUID, NAME, VERSION)]
sealed public class Plugin : BaseUnityPlugin
{
    public const string GUID = "mvv012.morepipejukenerfs";
    public const string NAME = "More Pipe Juke Nerfs";
    public const string VERSION = "0.7.0";

    private bool _isInit = false;
    internal static bool DebugWindowEnabled = false;

    internal static List<Hook> ManualHooks = [];
    internal static event Action? OnDisableEvent;

    public void OnEnable()
    {
        LogWrapper.OnEnable(base.Logger);

        DebugLogInfo("OnEnable");

        On.RainWorld.OnModsInit += RainWorld_OnModsInit;

        VesselShortcutCWT.ApplyHooks();
        VesselRemovalTracking.ApplyHooks();
        ShortcutPairTracking.OnEnable();
        NoticeCreatureUtils.ApplyHooks();
        GhostUtils.ApplyHooks();
        CreatureFixes.ApplyHooks();

        PipeJukeNotifier.OnEnable();
        PredictableShortcuts.OnEnable();

        PlayerShortcutTrackerCWT.ApplyHooks();
        ShortcutCounterReducer.ApplyHooks();

        RemixUtils.ApplyHooks();

#if DEBUG
        DebugOnEnable();
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

        OnDisableEvent?.Invoke();
        OnDisableEvent = null;

        LogWrapper.OnDisable();
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        if (_isInit) return;
        _isInit = true;

        MachineConnector.SetRegisteredOI(GUID, Options.Instance);
    }

#if DEBUG
    public void DebugOnEnable()
    {
        // For Rain Reloader, does nothing without it
        MachineConnector.SetRegisteredOI(GUID, Options.Instance);
        MachineConnector.ReloadConfig(Options.Instance);

        List<string> debugDeps = ["rwimgui", "maxi-mol.mousedrag", "warp"];
        List<string> disabledDeps = debugDeps.Where(dep => !ModManager.ActiveMods.Exists(mod => mod.id == dep)).ToList();

        if (disabledDeps.Count != 0)
        {
            DebugLog(LogLevel.Warning, $"Dependencies ({string.Join(", ", disabledDeps)}) for debug imgui menu are not enabled");
        }
        if (!LogWrapper.LogUtilsUsed)
        {
            DebugLog(LogLevel.Warning, $"LogUtils required for debug imgui menu is not enabled (Add as assembly or enable Log Manager)");
        }

        if (disabledDeps.Count == 0 && LogWrapper.LogUtilsUsed)
        {
            Debug.DebugImGUIWindow.OnEnable();
            Debug.Tests.TestRunner.OnEnable();
            Debug.Tests.RoomRealizingRestrictions.ApplyHooks();
            Debug.Tests.PlayerGraphicsInitiateSpritesFix.ApplyHooks();

            DebugWindowEnabled = true;
        }
    }
#endif
}