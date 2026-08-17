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

    public static TestableGroup Create(string name, TestCaseGroup? group = null)
    {
        return group == null
            ? new TestableGroup(name)
            : new TestableGroup(group, name);
    }
}
