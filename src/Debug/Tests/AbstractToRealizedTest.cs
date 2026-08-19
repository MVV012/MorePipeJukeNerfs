using LogUtils.Diagnostics;
using LogUtils.Diagnostics.Tests;
using RWCustom;

namespace MorePipeJukeNerfs.Debug.Tests;

internal class AbstractToRealizedTest : ShortcutTestBase
{
    internal record LocationInfo(
        string Region,
        string PlayerRoomName,
        WorldCoordinate PlayerCoord,
        WorldCoordinate TestedSpawn,
        WorldCoordinate OtherSpawn,
        WorldCoordinate TestedEnterCoord,
        IntVector2 OtherShortcut,
        int EnteringDelay
    ) : LocationInfoBase(Region, PlayerRoomName, PlayerCoord, TestedSpawn, OtherSpawn)
    {
        public static LocationInfo CC_C08_to_CC_A02 { get; } = new LocationInfo(
            Region: "CC",
            PlayerRoomName: "CC_A02",
            PlayerCoord: new WorldCoordinate(25, 25, 6, -1),
            TestedSpawn: new WorldCoordinate(27, 14, 14, -1),
            OtherSpawn: new WorldCoordinate(25, 43, 12, -1),
            TestedEnterCoord: new WorldCoordinate(25, 44, 12, 0),
            OtherShortcut: new IntVector2(48, 12),
            EnteringDelay: 8
        )
        {
            AbstractRooms = ["CC_C08"],
            RealizedRooms = ["CC_A02"]
        };

        public static LocationInfo CC_A02_to_CC_A12 { get; } = new LocationInfo(
            Region: "CC",
            PlayerRoomName: "CC_A12",
            PlayerCoord: new WorldCoordinate(23, 16, 17, -1),
            TestedSpawn: new WorldCoordinate(25, 6, 11, -1),
            OtherSpawn: new WorldCoordinate(23, 32, 17, -1),
            TestedEnterCoord: new WorldCoordinate(23, 32, 17, 1),
            OtherShortcut: new IntVector2(36, 18),
            EnteringDelay: 15
        )
        {
            AbstractRooms = ["CC_A02"],
            RealizedRooms = ["CC_A12"]
        };

        public static LocationInfo MPJNTST_A02_to_A01 { get; } = new LocationInfo(
            Region: "MPJNTST",
            PlayerRoomName: "MPJNTST_A01",
            PlayerCoord: new WorldCoordinate(871, 4, 10, -1),
            TestedSpawn: new WorldCoordinate(872, 6, 2, -1),
            OtherSpawn: new WorldCoordinate(871, 10, 4, -1),
            TestedEnterCoord: new WorldCoordinate(871, 12, 4, 0),
            OtherShortcut: new IntVector2(15, 4),
            EnteringDelay: 10
        )
        {
            AbstractRooms = ["MPJNTST_A02"],
            RealizedRooms = ["MPJNTST_A01"]
        };

        public static LocationInfo GetLocation(ShortcutTestLocationGroup locationGroup, CreatureTemplate.Type otherType)
        {
            return (locationGroup, otherType.value) switch
            {
                (ShortcutTestLocationGroup.CC, "Slugcat") => CC_A02_to_CC_A12,
                (ShortcutTestLocationGroup.CC, _) => CC_C08_to_CC_A02,
                (ShortcutTestLocationGroup.MPJNTST, _) => MPJNTST_A02_to_A01,
                _ => throw new ArgumentException("Invalid test group or shortcut type"),
            };
        }
    };

    public LocationInfo Location { get; }
    public override LocationInfoBase LocationBase => Location;

    public AbstractToRealizedTest(ShortcutTestInfo info, LocationInfo location) : base(info)
    {
        Location = location;
    }
    public AbstractToRealizedTest(TestCaseGroup group, ShortcutTestInfo info, LocationInfo location) : base(group, info)
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
            Tested.Move(Location.TestedEnterCoord);
        else
            Other.realizedCreature.SuckedIntoShortCut(Location.OtherShortcut, false);

        Game.Update(Location.EnteringDelay);

        if (OtherIsPlayer)
            RoomRealizingRestrictions.RemoveAllRestrictions();

        if (Info.First)
            Other.realizedCreature.SuckedIntoShortCut(Location.OtherShortcut, false);
        else
            Tested.Move(Location.TestedEnterCoord);

        return true;
    }

    public override void TestContent()
    {
        try
        {
            Game.UpdateWhile(() => Tested.realizedCreature == null || Tested.realizedCreature.inShortcut);
        }
        catch (Exception e)
        {
            this.Fail($"Unhandled exception: {e}");
            return;
        }

        AbstractRoom absRoom = Game.world.GetAbstractRoom(Location.TestedSpawn);
        if (!OtherIsPlayer && (absRoom.realizedRoom != null || !absRoom.KeepAbstract))
        {
            this.Fail("Tested creature room was not kept abstract");
            return;
        }

        bool creatureNoticed = Tested.TryGetRepresentation(Other, out var rep);
        AssertThat(creatureNoticed).IsTrue().OnFail("Creature is not noticed"); // 0
        if (!creatureNoticed) return;

        AssertThat(rep.lastSeenCoord).IsInRoom(Location.TestedSpawn.room); // 1

        if (rep is Tracker.ElaborateCreatureRepresentation elabRep)
        {
            AssertThat(elabRep.ghosts.Count).IsEqualTo(1); // 2

            Tracker.Ghost ghost = elabRep.ghosts[0];
            AssertThat(ghost.coord).IsInRoom(Location.TestedSpawn.room); // 3

            if (Info.First)
            {
                AssertThat(ghost.stopped).IsTrue(); // 4
                AssertThat(ghost.Pushable).IsFalse(); // 5
            }
            else
            {
                AssertThat(ghost.Pushable).IsTrue(); // 4
            }
        }

        if (rep.dynamicRelationship?.currentRelationship != null)
        {
            AssertThat(rep.dynamicRelationship.currentRelationship.type).DoesNotEqual(CreatureTemplate.Relationship.Type.SocialDependent); // 5/6
        }
    }

    public static AbstractToRealizedTest Create(ShortcutTestInfo info, LocationInfo location, TestCaseGroup? group = null)
    {
        return group == null
            ? new AbstractToRealizedTest(info, location)
            : new AbstractToRealizedTest(group, info, location);
    }
}
