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
        LogUtilsUsed = AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name.Equals("LogUtils", StringComparison.OrdinalIgnoreCase));

        if (LogUtilsUsed)
        {
            LogUtilsLoggerOnEnable();
            BepInExLogger.LogInfo($"LogUtils loaded, logging into {LogUtilsLoggerGetFilePath()}");
        }
#endif
    }

    public static void OnDisable()
    {
#if DEBUG
        if (LogUtilsUsed)
        {
            LogUtilsLoggerOnDisable();
        }
#endif
    }

    public static void Log(LogLevel level, object data)
    {
#if DEBUG
        if (LogUtilsUsed)
        {
            LogUtilsLoggerLog(level, data);
            return;
        }
#endif
        BepInExLogger.Log(level, data);
    }

#if DEBUG
    private static void LogUtilsLoggerOnEnable() => Debug.LogUtilsLogger.OnEnable();
    private static void LogUtilsLoggerOnDisable() => Debug.LogUtilsLogger.OnDisable();
    private static void LogUtilsLoggerLog(LogLevel level, object data)
    {
        if (Debug.DebugImGUIWindow.DontLogDebugInfo && level != LogLevel.Error)
        {
            return;
        }

        Debug.LogUtilsLogger.Global.Log.Log(level, data);
    }
    private static string LogUtilsLoggerGetFilePath() => Debug.LogUtilsLogger.ID.Properties.CurrentFilePath;
#endif
}
