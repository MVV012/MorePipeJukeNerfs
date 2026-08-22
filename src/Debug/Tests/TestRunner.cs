using LogUtils;
using LogUtils.Diagnostics;
using LogUtils.Diagnostics.Tests;
using LogUtils.Enums;
using LogUtils.Helpers;
using LogUtils.Policy;
using System.Diagnostics;

namespace MorePipeJukeNerfs.Debug.Tests;

internal class TestRunner
{
    public FormatEnums.FormatVerbosity ReportVerbosity { get; set; } = FormatEnums.FormatVerbosity.Verbose;
    public bool AssertNoLoggedErrors { get; set; } = true;

    public static LogID TestLogID = null!;
    public static Logger TestLogger = null!;
    public static Logger CombinedLogger = null!;

    public static void OnEnable()
    {
        TestLogID = new LogID("MorePipeJukeNerfsTests.log", LogAccess.FullAccess, register: true);
        LogUtilsLogger.UpdateProperties(TestLogID);
        TestLogger = new Logger(TestLogID);
        CombinedLogger = new Logger(LogTarget.Combiner.Combine(LogUtilsLogger.ID, TestLogID));
    }

    public static void OnDisable()
    {
        TestLogID.Unregister();
    }

    private AssertHandler _handler;
    private TestSuite _testSuite;

    public TestRunner()
    {
        _handler = new AssertHandler(TestLogger) { Behavior = AssertBehavior.DoNothing };
        _testSuite = new TestSuite { Handler = _handler };
    }

    public void RunTest(ITestable test, bool clearTestLog = true)
    {
        if (clearTestLog)
        {
            LogFile.StartNewSession(TestLogID);
        }

        TestCasePolicy.ReportVerbosity = ReportVerbosity;
        ShortcutTestBase.AssertNoLoggedErrors = AssertNoLoggedErrors;

        _testSuite = new TestSuite { Handler = _handler };
        _testSuite.Add(test);
        //CombinedLogger.LogDebug($"Starting test: {test.Name}");

        //using (new Stopwatch().BeginScope(CombinedLogger))
        {
            _testSuite.RunAllTests();
        }
    }

}
