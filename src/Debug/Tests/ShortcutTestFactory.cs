using LogUtils.Diagnostics.Tests;

namespace MorePipeJukeNerfs.Debug.Tests;

internal class ShortcutTestFactory
{
    public required CreatureTemplate.Type TestedType { get; set; }
    public required CreatureTemplate.Type OtherType { get; set; }

    public TestableGroup CreateAll(TestCaseGroup? group = null)
    {
        TestableGroup newGroup = TestableGroup.Create($"{TestedType} -> {OtherType}", group);

        CreateGroup(ShortcutTestType.NormalShortcut, newGroup);
        CreateGroup(ShortcutTestType.RealizedToRealized, newGroup);

        return newGroup;
    }

    public TestableGroup CreateGroup(ShortcutTestType type, TestCaseGroup? group = null)
    {
        TestableGroup newGroup = TestableGroup.Create($"{TestedType} -> {OtherType}, {type}", group);

        CreateTest(type, first: true, seen: false, newGroup);
        CreateTest(type, first: false, seen: false, newGroup);
        CreateTest(type, first: true, seen: true, newGroup);
        CreateTest(type, first: false, seen: true, newGroup);

        return newGroup;
    }

    public ShortcutTestBase CreateTest(ShortcutTestType type, bool first = true, bool seen = false, TestCaseGroup? group = null)
    {
        return type switch
        {
            ShortcutTestType.NormalShortcut or ShortcutTestType.RealizedToRealized => RealizedShortcutTest.Create(new ShortcutTestBase.ShortcutTestInfo()
            {
                Type = type,
                TestedType = TestedType,
                OtherType = OtherType,
                First = first,
                Seen = seen
            }, RealizedShortcutTest.LocationInfo.GetLocation(type), group),

            ShortcutTestType.AbstractToRealized => throw new NotImplementedException(),
            _ => throw new Exception("WAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")
        };
    }
}
