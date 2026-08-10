using RWCustom;
using LogUtils.Diagnostics;
using LogUtils.Diagnostics.Tests;
using System.Reflection;

namespace MorePipeJukeNerfs.Debug.Tests;

internal static class TestUtils
{
    extension(TestCase testCase)
    {
        public void Fail(string message)
        {
            testCase.AssertThat(false).Fail(new Condition.Message(message));
        }
    }

    private static FieldInfo? s_conditionResult;

    extension<T>(Condition<T> cond)
    {
        public void Fail(string message)
        {
            cond.Fail(new Condition.Message(message));
        }

        public Condition<T> OnFail(string message)
        {
            if (s_conditionResult == null)
            {
                s_conditionResult = typeof(Condition<T>).GetField("Result", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            Condition.Result result = (Condition.Result)s_conditionResult.GetValue(cond);
            result.Message = new Condition.Message(message);
            return cond;
        }
    }

    extension(Condition<WorldCoordinate> cond)
    {
        public Condition<WorldCoordinate> IsInRoom(AbstractRoom room)
        {
            if (!cond.ShouldProcess)
            {
                return cond;
            }
            MustBeInRoom(ref cond, room.index);
            return cond;
        }

        public Condition<WorldCoordinate> IsInRoom(int roomIndex)
        {
            if (!cond.ShouldProcess)
            {
                return cond;
            }
            MustBeInRoom(ref cond, roomIndex);
            return cond;
        }

        public Condition<WorldCoordinate> IsInRoom(string roomName)
        {
            if (!cond.ShouldProcess)
            {
                return cond;
            }
            if (RWGameUtils.TryGetRWGame(out var rw) && rw.world.GetAbstractRoom(roomName) is AbstractRoom room)
            {
                MustBeInRoom(ref cond, room.index);
            }
            else
            {
                cond.Fail($"Expected room ({roomName}) is not found");
            }
            return cond;
        }
    }

    public static void MustBeInRoom(ref Condition<WorldCoordinate> cond, int roomIndex)
    {
        if (cond.Value.room == roomIndex)
        {
            cond.Pass();
            return;
        }
        cond.Fail($"World coordinate is in wrong room. Expected: {roomIndex}, Got: {cond.Value}");
    }

    extension(Condition<IntVector2> cond)
    {
        public Condition<IntVector2> IsSameOrNextTo(IntVector2 other)
        {
            if (!cond.ShouldProcess)
            {
                return cond;
            }
            MustBeSameOrNextTo(ref cond, other);
            return cond;
        }
    }

    public static void MustBeSameOrNextTo(ref Condition<IntVector2> cond, IntVector2 other)
    {
        if (cond.Value.FloatDist(other) <= 1)
        {
            cond.Pass();
            return;
        }
        cond.Fail($"Tile coordinate must be in same tile or next to it. Expected: {other}, Got: {cond.Value}");
    }
}
