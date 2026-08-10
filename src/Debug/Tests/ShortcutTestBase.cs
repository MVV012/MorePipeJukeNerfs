using LogUtils.Diagnostics;
using LogUtils.Diagnostics.Tests;
using System.Diagnostics.CodeAnalysis;

namespace MorePipeJukeNerfs.Debug.Tests;

internal abstract class ShortcutTestBase : TestCase, ITestable
{
    public struct ShortcutTestInfo
    {
        public required CreatureTemplate.Type TestedType { get; init; }
        public required CreatureTemplate.Type OtherType { get; init; }
        public required bool First { get; init; }
        public required bool Seen { get; init; }
    }

    public ShortcutTestInfo Info { get; }
    public abstract string Region { get; }

    protected RainWorldGame _game = null!; // MemberNotNullWhenAttribute on Setup doesn't work for TestContent :(

    public ShortcutTestBase(ShortcutTestInfo info, string name) : base(name)
    {
        Info = info;
    }
    public ShortcutTestBase(TestCaseGroup group, ShortcutTestInfo info, string name) : base(group, name)
    {
        Info = info;
    }

    public virtual bool Setup()
    {
        if (!RWGameUtils.TryGetRWGame(out _game))
        {
            this.Fail("Game is not opened");
            return false;
        }
        if (_game.world.name != Region)
        {
            this.Fail($"Wrong region is loaded (expected: {Region}");
            return false;
        }

        _game.MurderEveryone();

        return true;
    }

    public void Test()
    {
        Condition.Result.ResetCount();

        if (!Setup())
        {
            return;
        }

        TestContent();

        _game.MurderEveryone();
    }

    [PostTest]
    public void ShowResults()
    {
        TestLogger.LogDebug(CreateReport());
    }

    public abstract void TestContent();
}
