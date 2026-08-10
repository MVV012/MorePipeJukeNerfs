using LogUtils.Diagnostics.Tests;

namespace MorePipeJukeNerfs.Debug.Tests;

internal class ShortcutTestFactory
{
    public required CreatureTemplate.Type TestedType { get; set; }
    public required CreatureTemplate.Type OtherType { get; set; }

    public TestableGroup CreateNormalGroup(TestCaseGroup? group = null)
    {
        TestableGroup normalGroup = group == null
            ? new TestableGroup($"{TestedType} -> {OtherType}, Normal shortcut")
            : new TestableGroup(group, $"{TestedType} -> {OtherType}, Normal shortcut");

        CreateNormal(normalGroup, first: true, seen: false);
        CreateNormal(normalGroup, first: false, seen: false);
        CreateNormal(normalGroup, first: true, seen: true);
        CreateNormal(normalGroup, first: false, seen: true);

        return normalGroup;
    }

    public NormalShortcutTest CreateNormal(TestCaseGroup? group = null, bool first = true, bool seen = false)
    {
        return group == null
            ? new NormalShortcutTest(new ShortcutTestBase.ShortcutTestInfo() {
                TestedType = TestedType,
                OtherType = OtherType,
                First = first,
                Seen = seen
            })
            : new NormalShortcutTest(group, new ShortcutTestBase.ShortcutTestInfo() {
                TestedType = TestedType,
                OtherType = OtherType,
                First = first,
                Seen = seen
            });
    }
}
