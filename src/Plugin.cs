using BepInEx;
using BepInEx.Bootstrap;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using MorePipeJukeNerfs.Remix;
using MorePipeJukeNerfs.Shortcuts;
using RWGenerators;
using System.Diagnostics;
using System.Security.Permissions;

// Allows access to private members
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace MorePipeJukeNerfs;

#if DEBUG
[BepInDependency("rwimgui", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("maxi-mol.mousedrag", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("LeeMoriya.Warp", BepInDependency.DependencyFlags.SoftDependency)]
#endif
[BepInAutoPlugin]
sealed public partial class Plugin : BaseUnityPlugin
{
    private bool _isInit = false;
    internal static bool DebugWindowEnabled = false;

    internal static List<Hook> ManualHooks = [];
    private static Action? OnDisableAction;
    [Conditional("DEBUG")]
    internal static void RegisterOnDisable(Action action) => OnDisableAction += action;

    public void OnEnable()
    {
        LogWrapper.OnEnable(base.Logger);

        try
        {
            LogDebug("OnEnable");

            On.RainWorld.OnModsInit += RainWorld_OnModsInit;

            AttachedFieldManager.ApplyHooks();

            VesselShortcutInfo.ApplyHooks();
            VesselRemovalTracking.ApplyHooks();
            ShortcutPairTracking.OnEnable();
            NoticeCreatureUtils.ApplyHooks();
            GhostPushable.ApplyHooks();
            CreatureFixes.ApplyHooks();

            PipeJukeNotifier.OnEnable();
            PredictableShortcuts.OnEnable();

            ShortcutCounterReducer.ApplyHooks();

            RemixUtils.ApplyHooks();

#if DEBUG
            DebugOnEnable();
#endif
            LogInfo("More Pipe Juke Nerfs initialized");
        }
        catch (Exception e)
        {
            LogError($"Falied to initialize More Pipe Juke Nerfs. Exception: {e}");
        }
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        if (_isInit) return;
        _isInit = true;

        MachineConnector.SetRegisteredOI(Id, RemixMenu.Instance);
    }

#if DEBUG
    public void OnDisable()
    {
        HookEndpointManager.RemoveAllOwnedBy(typeof(Plugin).Assembly);
        foreach (Hook hook in ManualHooks)
        {
            hook.Dispose();
        }
        ManualHooks.Clear();

        OnDisableAction?.Invoke();
        OnDisableAction = null;

        LogWrapper.OnDisable();
    }

    public void DebugOnEnable()
    {
        // For Rain Reloader, does nothing without it
        MachineConnector.SetRegisteredOI(Id, RemixMenu.Instance);
        MachineConnector.ReloadConfig(RemixMenu.Instance);

        string[] debugDeps = ["rwimgui", "maxi-mol.mousedrag", "LeeMoriya.Warp"];
        List<string> disabledDeps = [];
        foreach (string dep in debugDeps)
        {
            if (!Chainloader.PluginInfos.ContainsKey(dep))
            {
                disabledDeps.Add(dep);
            }
        }

        if (disabledDeps.Count != 0)
        {
            LogWarning($"Dependencies ({string.Join(", ", disabledDeps)}) for debug imgui menu are not enabled");
        }
        if (!LogWrapper.LogUtilsUsed)
        {
            LogWarning($"LogUtils required for debug imgui menu is not enabled (Add as assembly or enable Log Manager)");
        }

        if (disabledDeps.Count == 0 && LogWrapper.LogUtilsUsed)
        {
            TrueDebugOnEnable();
            DebugWindowEnabled = true;
        }
    }

    public void TrueDebugOnEnable()
    {
        Debug.DebugImGUIWindow.OnEnable();
        Debug.Tests.TestRunner.OnEnable();
        Debug.Tests.RoomRealizingRestrictions.ApplyHooks();
        Debug.Tests.PlayerGraphicsInitiateSpritesFix.ApplyHooks();
    }
#endif
}