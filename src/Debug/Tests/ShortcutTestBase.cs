using LogUtils.Diagnostics;
using LogUtils.Diagnostics.Tests;
using LogUtils.Enums;
using LogUtils.Events;
using RWCustom;

namespace MorePipeJukeNerfs.Debug.Tests;

public enum ShortcutTestType
{
    NormalShortcut,
    RealizedToRealized,
    AbstractToRealized
}

internal abstract class ShortcutTestBase : TestCase, ITestable
{
    public struct ShortcutTestInfo
    {
        public required CreatureTemplate.Type TestedType { get; init; }
        public required CreatureTemplate.Type OtherType { get; init; }
        public required bool First { get; init; }
        public required bool Seen { get; init; }
        public required ShortcutTestType Type { get; init; }
    }

    public record LocationInfoBase(
        string Region,
        string PlayerRoomName,
        WorldCoordinate PlayerCoord,
        WorldCoordinate TestedSpawn,
        WorldCoordinate OtherSpawn
    )
    {
        public string[] RealizedRooms { get; init; } = [];
        public string[] AbstractRooms { get; init; } = [];
    }

    public static bool AssertNoLoggedErrors = true;

    public ShortcutTestInfo Info { get; }
    public abstract LocationInfoBase LocationBase { get; }

    protected RainWorldGame Game { get; private set; } = null!; // MemberNotNullWhenAttribute on Setup doesn't work for TestContent :(
    protected AbstractCreature Tested { get; private set; } = null!;
    protected AbstractCreature Other { get; private set; } = null!;

    public ShortcutTestBase(ShortcutTestInfo info) : base(GetTestName(info))
    {
        Info = info;
    }
    public ShortcutTestBase(TestCaseGroup group, ShortcutTestInfo info) : base(group, GetTestName(info))
    {
        Info = info;
    }

    public virtual bool Setup()
    {
        if (!RWGameUtils.TryGetRWGame(out var rwgame))
        {
            this.Fail("Game is not opened");
            return false;
        }
        Game = rwgame;

        if (Game.world.region.name != LocationBase.Region || Game.FirstAnyPlayer.Room.name != LocationBase.PlayerRoomName)
        {
            WarpModMenu.newRegion = LocationBase.Region;
            WarpModMenu.newRoom = LocationBase.PlayerRoomName;
            WarpModMenu.warpActive = true;

            Game.UpdateWhile(() => Game.world.region.name != LocationBase.Region || Game.FirstAnyPlayer.Room.name != LocationBase.PlayerRoomName);

            Log.LogDebug("Successfully warped to testing region/room");
        }

        Game.MurderEveryone();

        RoomRealizingRestrictions.RemoveAllRestrictions();
        foreach (string name in LocationBase.RealizedRooms)
        {
            Game.world.GetAbstractRoom(name).RealizeAndRestrict(Game.world, Game);
        }
        foreach (string name in LocationBase.AbstractRooms)
        {
            Game.world.GetAbstractRoom(name).AbstractizeAndRestrict();
        }

        Game.FirstAnyPlayer.Move(LocationBase.PlayerCoord);
        if (Game.FirstRealizedPlayer != null)
        {
            foreach (var bodyChunk in Game.FirstRealizedPlayer.bodyChunks)
            {
                bodyChunk.HardSetPosition(LocationBase.PlayerCoord.MiddleOfTile);
            }
            MouseDrag.Health.ReviveCreature(Game.FirstRealizedPlayer);
        }

        Tested = Game.SpawnCreature(LocationBase.TestedSpawn, Info.TestedType);
        Other = Game.SpawnCreature(LocationBase.OtherSpawn, Info.OtherType);

        if (Info.Seen)
        {
            Tested.NoticeCreature(Other);
            Other.NoticeCreature(Tested);

            if (!Tested.TryGetRepresentation(Other, out var rep))
            {
                this.Fail("Failed to make tested creature aware of other one");
                return false;
            }
            if (!Other.TryGetRepresentation(Tested, out rep))
            {
                this.Fail("Failed to make other creature aware of tested one");
            }
        }

        return true;
    }

    public void Test()
    {
        Condition.Result.ResetCount();

        bool errorLogged = false;
        LogRequestEventHandler? logRequestHandler = null;
        if (AssertNoLoggedErrors)
        {
            logRequestHandler = request =>
            {
                if (request.Data.ID == LogUtilsLogger.ID && LogCategory.IsErrorCategory(request.Data.Category))
                {
                    errorLogged = true;
                }
            };
            LogRequestEvents.OnSubmit += logRequestHandler;
        }

        try
        {
            if (!Setup())
            {
                Game.MurderEveryone();
                return;
            }

            TestContent();
        }
        catch(Exception e)
        {
            Log.LogError($"Exception during testing: {e}");
            this.Fail("Exception during testing");
            return;
        }
        finally
        {
            RoomRealizingRestrictions.RemoveAllRestrictions();
        }

        if (AssertNoLoggedErrors)
        {
            AssertThat(errorLogged).IsFalse().OnFail($"Exception caught and logged to {Path.GetFileName(LogUtilsLogger.ID.Properties.CurrentFilePath)}");
            LogRequestEvents.OnSubmit -= logRequestHandler;
        }

        Game.MurderEveryone();
    }

    public abstract void TestContent();

    [PostTest]
    public void ShowResults()
    {
        TestLogger.LogDebug(CreateReport());
    }
    public static string GetTestName(ShortcutTestInfo info)
    {
        return $"{info.TestedType} -> {info.OtherType}, {info.Type}, {(info.First ? "First" : "Second")}, {(info.Seen ? "Seen" : "Unseen")}";
    }
}
