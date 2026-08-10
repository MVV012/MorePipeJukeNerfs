global using static MorePipeJukeNerfs.Debug.LogUtilsLogger.Global;

using LogUtils;
using LogUtils.Enums;

namespace MorePipeJukeNerfs.Debug;

internal static class LogUtilsLogger
{
    internal static class Global
    {
        public static Logger Log = null!;
    }

    public static LogID ID = null!;

    public static void OnEnable()
    {
        ID = new LogID("MorePipeJukeNerfs.log", LogAccess.FullAccess, register: true);
        ID.Properties.ShowCategories.Enable();

        Global.Log = new Logger(ID);
    }

    public static void OnDisable()
    {
        ID.Unregister();
    }

    public static void Log(BepInEx.Logging.LogLevel level, object data)
    {
        Global.Log.Log(level, data);
    }
}
