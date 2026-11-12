global using static MorePipeJukeNerfs.LogWrapper.Global;

using BepInEx.Logging;
using System.Diagnostics;

namespace MorePipeJukeNerfs;

internal static class LogWrapper
{
    internal static class Global
    {
        [Conditional("DEBUG")]
        public static void LogDebug(object data) => LogImpl(LogLevel.Debug, data);
        public static void LogInfo(object data) => LogImpl(LogLevel.Info, data);
        public static void LogMessage(object data) => LogImpl(LogLevel.Message, data);
        public static void LogWarning(object data) => LogBothImpl(LogLevel.Warning, data);
        public static void LogError(object data) => LogBothImpl(LogLevel.Error, data);
        public static void LogFatal(object data) => LogBothImpl(LogLevel.Fatal, data);
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

    public static void LogImpl(LogLevel level, object data)
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

    public static void LogBothImpl(LogLevel level, object data)
    {
#if DEBUG
        if (LogUtilsUsed)
        {
            LogUtilsLoggerLog(level, data);
        }
#endif
        BepInExLogger.Log(level, data);
    }

#if DEBUG
    private static void LogUtilsLoggerOnEnable() => Debug.LogUtilsLogger.OnEnable();
    private static void LogUtilsLoggerOnDisable() => Debug.LogUtilsLogger.OnDisable();
    private static void LogUtilsLoggerLog(LogLevel level, object data)
    {
        if (Plugin.DebugWindowEnabled && Debug.DebugImGUIWindow.DontLogDebugInfo && (level == LogLevel.Info || level == LogLevel.Debug))
        {
            return;
        }

        Debug.LogUtilsLogger.Global.Log.Log(level, data);
    }
    private static string LogUtilsLoggerGetFilePath() => Debug.LogUtilsLogger.ID.Properties.CurrentFilePath;
#endif
}
