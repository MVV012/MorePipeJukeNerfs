using DevConsole;
using LogUtils.Diagnostics;
using LogUtils.Diagnostics.Tests;
using RWCustom;

namespace MorePipeJukeNerfs.Debug.Tests;

internal class NormalShortcutTest : ShortcutTestBase
{
    internal record LocationInfo(
        string region,
        int roomIndex,
        IntVector2 testedSpawn,
        IntVector2 testedShortcut,
        IntVector2 otherSpawn,
        IntVector2 otherShortcut,
        IntVector2 playerPos,
        int enteringDelay
    )
    {
        public static LocationInfo CC_A02 => new LocationInfo(
            region: "CC",
            roomIndex: 25,
            testedSpawn: new IntVector2(28, 18),
            testedShortcut: new IntVector2(31, 15),
            otherSpawn: new IntVector2(36, 5),
            otherShortcut: new IntVector2(38, 3),
            playerPos: new IntVector2(25, 5),
            enteringDelay: 20
        );
    };

    public LocationInfo Location { get; init; } = LocationInfo.CC_A02;

    public override string Region => Location.region;

    private Creature _tested = null!;
    private Creature _other = null!;

    public NormalShortcutTest(ShortcutTestInfo info) : base(info, GetTestName(info)) {}
    public NormalShortcutTest(TestCaseGroup group, ShortcutTestInfo info) : base(group, info, GetTestName(info)) {}

    public override bool Setup()
    {
        if (!base.Setup())
        {
            return false;
        }

        AbstractRoom absRoom = _game.world.GetAbstractRoom(Location.roomIndex);
        if (absRoom.realizedRoom == null)
        {
            this.Fail($"Room ({absRoom.name}) is not realized");
            return false;
        }

        foreach (var bodyChunk in _game.FirstRealizedPlayer.bodyChunks)
        {
            bodyChunk.HardSetPosition(Location.playerPos.MiddleOfTile);
        }

        _tested = _game.SpawnCreature(absRoom, Location.testedSpawn, Info.TestedType).realizedCreature;
        _other = _game.SpawnCreature(absRoom, Location.otherSpawn, Info.OtherType).realizedCreature;

        if (Info.Seen)
        {
            _tested.abstractCreature.NoticeCreature(_other.abstractCreature);
        }

        if (Info.First)
            _tested.SuckedIntoShortCut(Location.testedShortcut, false);
        else
            _other.SuckedIntoShortCut(Location.otherShortcut, false);

        _game.Update(Location.enteringDelay);

        if (Info.First)
            _other.SuckedIntoShortCut(Location.otherShortcut, false);
        else
            _tested.SuckedIntoShortCut(Location.testedShortcut, false);

        return true;
    }

    public override void TestContent()
    {
        try
        {
            _game.UpdateWhile(() => _tested.inShortcut);
        }
        catch (Exception e)
        {
            this.Fail($"Unhandled exception: {e}");
            return;
        }

        bool creatureNoticed = _tested.abstractCreature.TryGetRepresentation(_other.abstractCreature, out var rep);
        AssertThat(creatureNoticed).IsTrue().OnFail("Creature is not noticed"); // 0
        if (!creatureNoticed) return;

        AssertThat(rep.lastSeenCoord).IsInRoom(Location.roomIndex); // 1
        AssertThat(rep.lastSeenCoord.Tile).IsSameOrNextTo(Location.testedShortcut); // 2

        if (rep is Tracker.ElaborateCreatureRepresentation elabRep)
        {
            AssertThat(elabRep.ghosts.Count).IsEqualTo(1); // 3

            Tracker.Ghost ghost = elabRep.ghosts[0];
            AssertThat(ghost.coord).IsInRoom(Location.roomIndex); // 4
            AssertThat(ghost.coord.Tile).IsSameOrNextTo(rep.lastSeenCoord.Tile); // 5
            AssertThat(ghost.pos.TilePosition).IsSameOrNextTo(rep.lastSeenCoord.Tile); // 6

            if (Info.First)
            {
                AssertThat(ghost.stopped).IsTrue(); // 7
                AssertThat(ghost.Pushable).IsFalse(); // 8
            }
            else
            {
                AssertThat(ghost.Pushable).IsTrue(); // 7
            }
        }

        if (rep.dynamicRelationship?.currentRelationship != null)
        {
            AssertThat(rep.dynamicRelationship.currentRelationship.type).DoesNotEqual(CreatureTemplate.Relationship.Type.SocialDependent); // 8/9
        }

        // TODO: add check for handled exceptions from logger
    }

    public static string GetTestName(ShortcutTestInfo data)
    {
        return $"{data.TestedType} -> {data.OtherType}, {(data.First ? "First" : "Second")}, {(data.Seen ? "Seen" : "Unseen")}, Normal shortcut";
    }
}
