using BepInEx;
using BepInEx.Logging;
using System.Security.Permissions;


// Allows access to private members
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618


namespace NoMorePipeJuking;


[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
sealed public class Plugin : BaseUnityPlugin
{
    public const string PLUGIN_GUID = "mvv012.nomorepipejuking";
    public const string PLUGIN_NAME = "No More Pipe Juking";
    public const string PLUGIN_VERSION = "0.1.0";

    private bool _isInit = false;

    public static new ManualLogSource Logger = null!;


    public void OnEnable()
    {
        Logger = base.Logger;

        On.RainWorld.OnModsInit += RainWorld_OnModsInit;

        ShortcutCreatureTracking.ApplyHooks();
        NoticeCreatureUtils.ApplyHooks();
    }

    public void OnDisable()
    {
        On.RainWorld.OnModsInit -= RainWorld_OnModsInit;

        ShortcutCreatureTracking.RemoveHooks();
        NoticeCreatureUtils.RemoveHooks();
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        if (_isInit) return;
        _isInit = true;

        // MachineConnector.SetRegisteredOI(PLUGIN_GUID, Options.Instance);
    }
}