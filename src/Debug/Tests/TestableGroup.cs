using LogUtils.Diagnostics.Tests;

namespace MorePipeJukeNerfs.Debug.Tests;

internal class TestableGroup : TestCaseGroup, ITestable
{
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
        TestLogger.LogDebug(CreateReport());
    }
}
