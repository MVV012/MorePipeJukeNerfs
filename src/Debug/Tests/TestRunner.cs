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

    public void RunTests(params IEnumerable<ITestable> tests)
    {
        LogID testLogID = new LogID("MorePipeJukeNerfsTests.log", LogAccess.FullAccess, register: true);
        Logger testLogger = new Logger(testLogID);
        LogFile.StartNewSession(testLogID);

        AssertHandler assertHandler = new AssertHandler(testLogger);
        assertHandler.Behavior = AssertBehavior.DoNothing;

        TestCasePolicy.ReportVerbosity = ReportVerbosity;

        TestSuite testSuite = new TestSuite();
        testSuite.Handler = assertHandler;

        foreach (ITestable test in tests)
        {
            testSuite.Add(test);
        }

        DebugLogInfo("STARTING TESTS");
        testSuite.RunAllTests();
    }
}
