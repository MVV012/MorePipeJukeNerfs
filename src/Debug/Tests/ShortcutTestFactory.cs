using LogUtils.Diagnostics.Tests;

namespace MorePipeJukeNerfs.Debug.Tests;

internal class ShortcutTestFactory(CreatureTemplate.Type TestedType, CreatureTemplate.Type OtherType)
{
    public ShortcutTestLocationGroup LocationGroup { get; init; } = ShortcutTestLocationGroup.CC;

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
                RealizedShortcutTest.LocationInfo.GetLocation(LocationGroup, type),
                group
            ),
            ShortcutTestType.AbstractToRealized => AbstractToRealizedTest.Create(
                info,
                AbstractToRealizedTest.LocationInfo.GetLocation(LocationGroup, OtherType),
                group
            ),
            _ => throw new Exception("WAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")
        };
    }

    public TestableGroup CreateAllTestsForPair(TestCaseGroup? group = null)
    {
        TestableGroup newGroup = TestableGroup.Create($"{TestedType} <-> {OtherType}", group);

        CreateAll(newGroup);
        new ShortcutTestFactory(OtherType, TestedType) { LocationGroup = LocationGroup }.CreateAll(newGroup);

        return newGroup;
    }

    public static TestableGroup CreateAllForTested(CreatureTemplate.Type testedType, CreatureTemplate.Type[] otherTypes, ShortcutTestLocationGroup locationGroup, TestCaseGroup? group = null)
    {
        if (otherTypes.Length == 1)
        {
            return new ShortcutTestFactory(testedType, otherTypes[0]) { LocationGroup = locationGroup }.CreateAll(group);
        }

        TestableGroup newGroup = TestableGroup.Create($"{testedType} -> {string.Join(", ", otherTypes.Select(ct => ct.value))}", group);

        foreach (var other in otherTypes)
        {
            new ShortcutTestFactory(testedType, other) { LocationGroup = locationGroup }.CreateAll(newGroup);
        }

        return newGroup;
    }
}
