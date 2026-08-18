using ImGuiNET;
using LogUtils;
using LogUtils.Diagnostics;
using LogUtils.Enums;
using LogUtils.Helpers;
using MonoMod.RuntimeDetour;
using MorePipeJukeNerfs.Debug.Tests;
using MorePipeJukeNerfs.Shortcuts;
using RWCustom;
using RWIMGUI.API;
using System.Diagnostics;

namespace MorePipeJukeNerfs.Debug;

internal class DebugImGUIWindow
{
    private unsafe static delegate* managed<ref IntPtr, ref uint, ref uint, void> s_customMenuCallback;
    public static void OnEnable()
    {
        unsafe
        {
            s_customMenuCallback = &ShowWindow;
            ImGUIAPI.AddCustomMenuCallback(s_customMenuCallback);
        }
        Plugin.ManualHooks.Add(new Hook(
            typeof(IMGUIContext).GetMethod(nameof(IMGUIContext.BlockWMEvent)),
            BlockWMEvent_Hook
        ));
    }

    public static void OnDisable()
    {
        unsafe
        {
            ImGUIAPI.RemoveCustomMenuCallback(s_customMenuCallback);
            s_customMenuCallback = null;
        }
    }

    private static bool BlockWMEvent_Hook(Func<IMGUIContext, bool> orig, IMGUIContext self)
    {
        return orig(self) && (ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow) || ImGui.IsAnyItemHovered());
    }

    public static void ShowWindow(ref IntPtr IDXGISwapChain, ref uint SyncInterval, ref uint Flags)
    {
        ImGui.GetIO().MouseDrawCursor = false;
        if (ImGui.Begin("More Pipe Juke Nerfs Debug"))
        {
            WindowContent();
        }
        ImGui.End();
    }

    private static int updates = 40;
    private static CreatureTemplate.Type testedCreature = CreatureTemplate.Type.Scavenger;
    private static CreatureTemplate.Type otherCreature = CreatureTemplate.Type.RedCentipede;
    private static bool first = true;
    private static bool seen = false;
    private static ShortcutTestType testType = ShortcutTestType.NormalShortcut;
    private static FormatEnums.FormatVerbosity reportVerbosity = FormatEnums.FormatVerbosity.Compact;
    private static bool assertNoLoggedErrors = true;
    private static void WindowContent()
    {
        if (!RWGameUtils.TryGetRWGame(out RainWorldGame game))
        {
            ImGui.Text("Game is not opened!");
            return;
        }

        if (ImGui.TreeNodeEx("Wawa", ImGuiTreeNodeFlags.Framed))
        {
            ImGui.Unindent();

            if (ImGui.Button("Murder everyone"))
            {
                game.MurderEveryone();
            }
            ImGui.SameLine();
            if (ImGui.Button("Clear log"))
            {
                LogFile.StartNewSession(LogUtilsLogger.ID);
            }

            ImGui.SetNextItemWidth(215);
            ImGui.InputInt("##Update count", ref updates);
            ImGui.SameLine();
            if (ImGui.Button($"Update game {updates} times###update_button"))
            {
                RunOnMainThread(() =>
                {
                    Log.LogDebug($"Running {updates} RainWorldGame updates in {game.FirstAnyPlayer.Room.name}");
                    using (new Stopwatch().BeginScope(Log))
                    {
                        game.Update(updates);
                    }
                });
            }

            ImGui.Indent();
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeEx("Testing", ImGuiTreeNodeFlags.Framed))
        {
            ImGui.Unindent();

            ImGUIComponents.CreatureTemplatePicker("Tested creature", ref testedCreature);
            ImGUIComponents.CreatureTemplatePicker("Other creature", ref otherCreature, withSlugcat: true);
            ImGUIComponents.EnumPicker("Test type", ref testType);
            ImGui.Checkbox("First", ref first);
            ImGui.Checkbox("Seen", ref seen);

            ImGui.Separator();

            ImGUIComponents.EnumPicker("Report verbosity", ref reportVerbosity);
            ImGui.Checkbox("Assert no logged errors", ref assertNoLoggedErrors);

            if (game.GamePaused || game.FirstAnyPlayer.realizedCreature == null || game.FirstRealizedPlayer.inShortcut)
            {
                ImGui.BeginDisabled();
            }

            if (ImGui.Button("Run test"))
            {
                RunOnMainThread(() => {
                    ShortcutTestFactory factory = new(testedCreature, otherCreature);
                    TestRunner runner = new() { ReportVerbosity = reportVerbosity, AssertNoLoggedErrors = assertNoLoggedErrors };
                    runner.RunTests(factory.CreateTest(testType, first, seen));
                });
            }
            ImGui.SameLine();
            if (ImGui.Button("Run all tests"))
            {
                RunOnMainThread(() => {
                    ShortcutTestFactory factory = new(testedCreature, otherCreature);
                    TestRunner runner = new() { ReportVerbosity = reportVerbosity, AssertNoLoggedErrors = assertNoLoggedErrors };
                    runner.RunTests(factory.CreateAll());
                });
            }

            ImGui.SameLine();
            if (otherCreature == CreatureTemplate.Type.Slugcat)
                ImGui.BeginDisabled();

            if (ImGui.Button("Run all tests for pair"))
            {
                RunOnMainThread(() => {
                    TestRunner runner = new() { ReportVerbosity = reportVerbosity, AssertNoLoggedErrors = assertNoLoggedErrors };
                    runner.RunTests(ShortcutTestFactory.CreateAllTestsForPair(testedCreature, otherCreature));
                });
            }

            if (otherCreature == CreatureTemplate.Type.Slugcat)
                ImGui.EndDisabled();

            if (ImGui.Button("Setup test"))
            {
                RunOnMainThread(() => {
                    ShortcutTestFactory factory = new(testedCreature, otherCreature);
                    factory.CreateTest(testType, first, seen).Setup();
                });
            }

            if (game.GamePaused || game.FirstAnyPlayer.realizedCreature == null || game.FirstRealizedPlayer.inShortcut)
            {
                ImGui.EndDisabled();
            }

            ImGui.SameLine();
            if (ImGui.Button("Reset realizing restrictions"))
            {
                RoomRealizingRestrictions.RemoveAllRestrictions();
            }

            ImGui.Indent();
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeEx("Creatures", ImGuiTreeNodeFlags.Framed))
        {
            ImGui.Unindent();

            foreach (var cr in game.world.abstractRooms
                .SelectMany(room => room.creatures)
                .Where(cr => !cr.creatureTemplate.smallCreature)
                .OrderBy(cr => cr.ID.number))
            {
                ImGUIComponents.CreatureInfo(cr);
            }

            ImGui.Indent();
            ImGui.TreePop();
        }
    }

    private static void RunOnMainThread(Action action)
    {
        UtilityCore.Scheduler.Schedule(action, frameInterval: 1, invokeLimit: 1);
    }
}
