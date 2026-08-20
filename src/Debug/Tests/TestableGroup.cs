using LogUtils.Diagnostics.Tests;

namespace MorePipeJukeNerfs.Debug.Tests;

internal class TestableGroup : TestCaseGroup, ITestable
{
    public bool DontLogReport { get; set; } = false;

    public IEnumerable<TestableGroup> TestableCases => Cases.OfType<TestableGroup>();

    public TestableGroup(string name) : base(name) {}
    public TestableGroup(TestCaseGroup group, string name) : base(group, name) {}

    public void Test()
    {
        foreach (ITestable testable in Cases.OfType<ITestable>())
        {
            testable.Test();
        }
    }

    [PostTest]
    public void ShowResults()
    {
        if (!DontLogReport)
        {
            TestLogger.LogDebug(CreateReport());
        }
    }

    public static TestableGroup Create(string name, TestCaseGroup? group = null, bool dontLogReport = false)
    {
        return group == null
            ? new TestableGroup(name) { DontLogReport = dontLogReport }
            : new TestableGroup(group, name) { DontLogReport = dontLogReport };
    }
}
