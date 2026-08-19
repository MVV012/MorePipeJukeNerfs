using LogUtils.Diagnostics;
using LogUtils.Diagnostics.Tests;
using RWCustom;

namespace MorePipeJukeNerfs.Debug.Tests;

internal class RealizedShortcutTest : ShortcutTestBase
{
    internal record LocationInfo(
        string Region,
        string PlayerRoomName,
        WorldCoordinate PlayerCoord,
        WorldCoordinate TestedSpawn,
        WorldCoordinate OtherSpawn,
        IntVector2 TestedShortcut,
        IntVector2 OtherShortcut,
        int EnteringDelay
    ) : LocationInfoBase(Region, PlayerRoomName, PlayerCoord, TestedSpawn, OtherSpawn)
    {
        public static LocationInfo CC_A02 { get; } = new LocationInfo(
            Region: "CC",
            PlayerRoomName: "CC_A02",
            PlayerCoord: new WorldCoordinate(25, 25, 6, -1),
            TestedSpawn: new WorldCoordinate(25, 28, 18, -1),
            TestedShortcut: new IntVector2(31, 15),
            OtherSpawn: new WorldCoordinate(25, 36, 5, -1),
            OtherShortcut: new IntVector2(38, 3),
            EnteringDelay: 20
        )
        {
            RealizedRooms = ["CC_A02"]
        };

        public static LocationInfo CC_A02_to_CC_A12 { get; } = new LocationInfo(
            Region: "CC",
            PlayerRoomName: "CC_A02",
            PlayerCoord: new WorldCoordinate(25, 3, 27, -1),
            TestedSpawn: new WorldCoordinate(23, 33, 18, -1),
            TestedShortcut: new IntVector2(36, 18),
            OtherSpawn: new WorldCoordinate(25, 6, 11, -1),
            OtherShortcut: new IntVector2(3, 11),
            EnteringDelay: 20
        )
        {
            RealizedRooms = ["CC_A02", "CC_A12"]
        };

        public static LocationInfo MPJNTST_A01 { get; } = new LocationInfo(
            Region: "MPJNTST",
            PlayerRoomName: "MPJNTST_A01",
            PlayerCoord: new WorldCoordinate(871, 4, 10, -1),
            TestedSpawn: new WorldCoordinate(871, 3, 4, -1),
            TestedShortcut: new IntVector2(4, 1),
            OtherSpawn: new WorldCoordinate(871, 10, 4, -1),
            OtherShortcut: new IntVector2(8, 1),
            EnteringDelay: 6
        )
        {
            RealizedRooms = ["MPJNTST_A01"]
        };

        public static LocationInfo MPJNTST_A02_to_A01 { get; } = new LocationInfo(
            Region: "MPJNTST",
            PlayerRoomName: "MPJNTST_A01",
            PlayerCoord: new WorldCoordinate(871, 4, 10, -1),
            TestedSpawn: new WorldCoordinate(872, 6, 2, -1),
            TestedShortcut: new IntVector2(2, 2),
            OtherSpawn: new WorldCoordinate(871, 10, 4, -1),
            OtherShortcut: new IntVector2(15, 4),
            EnteringDelay: 10
        )
        {
            RealizedRooms = ["MPJNTST_A02", "MPJNTST_A01"]
        };

        public static LocationInfo GetLocation(ShortcutTestLocationGroup locationGroup, ShortcutTestType type)
        {
            return (locationGroup, type) switch
            {
                (ShortcutTestLocationGroup.CC, ShortcutTestType.NormalShortcut) => CC_A02,
                (ShortcutTestLocationGroup.CC, ShortcutTestType.RealizedToRealized) => CC_A02_to_CC_A12,
                (ShortcutTestLocationGroup.MPJNTST, ShortcutTestType.NormalShortcut) => MPJNTST_A01,
                (ShortcutTestLocationGroup.MPJNTST, ShortcutTestType.RealizedToRealized) => MPJNTST_A02_to_A01,
                _ => throw new ArgumentException("Invalid test group or shortcut type"),
            };
        }
    };

    public LocationInfo Location { get; }
    public override LocationInfoBase LocationBase => Location;

    public RealizedShortcutTest(ShortcutTestInfo info, LocationInfo location) : base(info)
    {
        Location = location;
    }
    public RealizedShortcutTest(TestCaseGroup group, ShortcutTestInfo info, LocationInfo location) : base(group, info)
    {
        Location = location;
    }

    public override bool Setup()
    {
        if (!base.Setup())
        {
            return false;
        }

        if (Info.First)
            Tested.realizedCreature.SuckedIntoShortCut(Location.TestedShortcut, false);
        else
            Other.realizedCreature.SuckedIntoShortCut(Location.OtherShortcut, false);

        Game.Update(Location.EnteringDelay);

        if (Info.First)
            Other.realizedCreature.SuckedIntoShortCut(Location.OtherShortcut, false);
        else
            Tested.realizedCreature.SuckedIntoShortCut(Location.TestedShortcut, false);

        return true;
    }

    public override void TestContent()
    {
        try
        {
            Game.UpdateWhile(() => Tested.realizedCreature.inShortcut);
        }
        catch (Exception e)
        {
            this.Fail($"Unhandled exception: {e}");
            return;
        }

        bool creatureNoticed = Tested.TryGetRepresentation(Other, out var rep);
        AssertThat(creatureNoticed).IsTrue().OnFail("Creature is not noticed"); // 0
        if (!creatureNoticed) return;

        AssertThat(rep.lastSeenCoord).IsInRoom(Location.TestedSpawn.room); // 1
        AssertThat(rep.lastSeenCoord.Tile).IsSameOrNextTo(Location.TestedShortcut); // 2

        if (rep is Tracker.ElaborateCreatureRepresentation elabRep)
        {
            AssertThat(elabRep.ghosts.Count).IsNotZero(); // 3

            Tracker.Ghost ghost = elabRep.ghosts[0];
            AssertThat(ghost.coord).IsInRoom(Location.TestedSpawn.room); // 4
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
    }

    public static RealizedShortcutTest Create(ShortcutTestInfo info, LocationInfo location, TestCaseGroup? group = null)
    {
        return group == null
            ? new RealizedShortcutTest(info, location)
            : new RealizedShortcutTest(group, info, location);
    }
}
