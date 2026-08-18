using LogUtils.Diagnostics.Tests;

namespace MorePipeJukeNerfs.Debug.Tests;

internal class ShortcutTestFactory(CreatureTemplate.Type TestedType, CreatureTemplate.Type OtherType)
{
    public ShortcutTestLocationGroup TestGroup { get; init; } = ShortcutTestLocationGroup.CC;

    public TestableGroup CreateAll(TestCaseGroup? group = null)
    {
        TestableGroup newGroup = TestableGroup.Create($"{TestedType} -> {OtherType}", group);

        CreateGroup(ShortcutTestType.NormalShortcut, newGroup);
        CreateGroup(ShortcutTestType.RealizedToRealized, newGroup);
        CreateGroup(ShortcutTestType.AbstractToRealized, newGroup);

        return newGroup;
    }

    public TestableGroup CreateGroup(ShortcutTestType type, TestCaseGroup? group = null)
    {
        TestableGroup newGroup = TestableGroup.Create($"{TestedType} -> {OtherType}, {type}", group);

        CreateTest(type, first: true, seen: false, newGroup);
        CreateTest(type, first: false, seen: false, newGroup);
        if (type != ShortcutTestType.AbstractToRealized)
        {
            CreateTest(type, first: true, seen: true, newGroup);
            CreateTest(type, first: false, seen: true, newGroup);
        }

        return newGroup;
    }

    public ShortcutTestBase CreateTest(ShortcutTestType type, bool first = true, bool seen = false, TestCaseGroup? group = null)
    {
        ShortcutTestBase.ShortcutTestInfo info = new ShortcutTestBase.ShortcutTestInfo() {
            Type = type,
            TestedType = TestedType,
            OtherType = OtherType,
            First = first,
            Seen = seen
        };

        return type switch
        {
            ShortcutTestType.NormalShortcut or ShortcutTestType.RealizedToRealized => RealizedShortcutTest.Create(
                info,
                RealizedShortcutTest.LocationInfo.GetLocation(TestGroup, type),
                group
            ),
            ShortcutTestType.AbstractToRealized => AbstractToRealizedTest.Create(
                info,
                AbstractToRealizedTest.LocationInfo.GetLocation(TestGroup, OtherType),
                group
            ),
            _ => throw new Exception("WAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")
        };
    }

    public TestableGroup CreateAllTestsForPair(TestCaseGroup? group = null)
    {
        TestableGroup newGroup = TestableGroup.Create($"{TestedType} <-> {OtherType}", group);

        CreateAll(newGroup);
        new ShortcutTestFactory(OtherType, TestedType) { TestGroup = TestGroup }.CreateAll(newGroup);

        return newGroup;
    }
}
