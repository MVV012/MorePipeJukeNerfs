global using static MorePipeJukeNerfs.LogWrapper.Global;

using BepInEx.Logging;
using System.Diagnostics;

namespace MorePipeJukeNerfs;

internal static class LogWrapper
{
    internal static class Global
    {
        public static void ReleaseLog(LogLevel level, object data)
        {
            Log(level, data);
        }

        [Conditional("DEBUG")]
        public static void DebugLog(LogLevel level, object data)
        {
            Log(level, data);
        }

        [Conditional("DEBUG")]
        public static void DebugLogInfo(object data)
        {
            Log(LogLevel.Info, data);
        }
    }

    public static bool LogUtilsUsed = false;
    public static ManualLogSource BepInExLogger = null!;

    public static void OnEnable(ManualLogSource bepInExLogger)
    {
        BepInExLogger = bepInExLogger;

#if DEBUG
        LogUtilsUsed = AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name.Contains("LogUtils"));

        if (LogUtilsUsed)
        {
            Debug.LogUtilsLogger.OnEnable();

            BepInExLogger.LogInfo($"LogUtils loaded, logging into {Debug.LogUtilsLogger.ID.Properties.CurrentFilePath}");
        }
#endif
    }

    public static void OnDisable()
    {
#if DEBUG
        if (LogUtilsUsed)
        {
            Debug.LogUtilsLogger.OnDisable();
        }
#endif
    }

    public static void Log(LogLevel level, object data)
    {
#if DEBUG
        if (LogUtilsUsed)
        {
            Debug.LogUtilsLogger.Log(level, data);
            return;
        }
#endif
        BepInExLogger.Log(level, data);
    }
}
