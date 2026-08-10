using LogUtils;
using LogUtils.Diagnostics;
using LogUtils.Diagnostics.Tests;
using LogUtils.Enums;
using LogUtils.Helpers;
using LogUtils.Policy;

namespace MorePipeJukeNerfs.Debug.Tests;

internal class TestRunner
{
    public FormatEnums.FormatVerbosity ReportVerbosity { get; set; } = FormatEnums.FormatVerbosity.Verbose;

    public static bool IsInit = false;
    public static LogID TestLogID = null!;
    public static Logger TestLogger = null!;
    public static Logger CombinedLogger = null!;

    private static void Init()
    {
        TestLogID = new LogID("MorePipeJukeNerfsTests.log", LogAccess.FullAccess, register: true);
        LogUtilsLogger.RemoveIntoOutroMessages(TestLogID);
        TestLogger = new Logger(TestLogID);
        CombinedLogger = new Logger(LogTarget.Combiner.Combine(LogUtilsLogger.ID, TestLogID));
    }

    public void RunTests(params ITestable[] tests)
    {
        if (!IsInit)
        {
            Init();
        }

        LogFile.StartNewSession(TestLogID);

        AssertHandler assertHandler = new AssertHandler(TestLogger);
        assertHandler.Behavior = AssertBehavior.DoNothing;

        TestCasePolicy.ReportVerbosity = ReportVerbosity;

        TestSuite testSuite = new TestSuite();
        testSuite.Handler = assertHandler;

        foreach (ITestable test in tests)
        {
            testSuite.Add(test);
        }

        CombinedLogger.LogDebug(GetLogMessage(tests));
        testSuite.RunAllTests();
    }

    public static string GetLogMessage(params ITestable[] tests)
    {
        string testNames = string.Join("; ", tests.Select(test => test switch {
            TestCase @case => @case.Name,
            _ => "Unnamed test"
        }));
        int limit = 50;
        if (testNames.Length > limit) testNames = testNames[..limit] + "...";
        return $"Starting {tests.Length} testable{(tests.Length == 1 ? "" : "s")}: {testNames}";
    }
}
