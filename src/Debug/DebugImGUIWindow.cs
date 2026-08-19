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
using UnityEngine;

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
        if (runningTests) return false;

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
    private static ShortcutTestLocationGroup locationGroup = ShortcutTestLocationGroup.CC;
    private static bool[]? testedCreatures = null;
    private static bool[]? otherCreatures = null;

    private static bool runningTests = false;
    private static int currentTestIndex = -1;
    private static long startTicks = -1;
    private static Stopwatch testStopwatch = null!;
    private static string? testRunningInfo = null;

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

            ImGUIComponents.EnumPicker("Location", ref locationGroup);

            ImGui.Separator();

            ImGUIComponents.EnumPicker("Report verbosity", ref reportVerbosity);
            ImGui.Checkbox("Assert no logged errors", ref assertNoLoggedErrors);

            if (ImGui.BeginTabBar("testingTabBar"))
            {
                if (ImGui.BeginTabItem("Single test"))
                {
                    ImGUIComponents.CreatureTemplatePicker("Tested creature", ref testedCreature);
                    ImGUIComponents.CreatureTemplatePicker("Other creature", ref otherCreature, withSlugcat: true);
                    ImGUIComponents.EnumPicker("Test type", ref testType);
                    ImGui.Checkbox("First", ref first);
                    ImGui.Checkbox("Seen", ref seen);

                    if (game.GamePaused || game.FirstAnyPlayer.realizedCreature == null || game.FirstRealizedPlayer.inShortcut)
                    {
                        ImGui.BeginDisabled();
                    }

                    if (ImGui.Button("Run test"))
                    {
                        RunOnMainThread(() => {
                            ShortcutTestFactory factory = new(testedCreature, otherCreature) { LocationGroup = locationGroup };
                            TestRunner runner = new() { ReportVerbosity = reportVerbosity, AssertNoLoggedErrors = assertNoLoggedErrors };
                            runner.RunTest(factory.CreateTest(testType, first, seen));
                        });
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Run all tests"))
                    {
                        RunOnMainThread(() => {
                            ShortcutTestFactory factory = new(testedCreature, otherCreature) { LocationGroup = locationGroup };
                            TestRunner runner = new() { ReportVerbosity = reportVerbosity, AssertNoLoggedErrors = assertNoLoggedErrors };
                            runner.RunTest(factory.CreateAll());
                        });
                    }

                    ImGui.SameLine();
                    if (otherCreature == CreatureTemplate.Type.Slugcat)
                        ImGui.BeginDisabled();

                    if (ImGui.Button("Run all tests for pair"))
                    {
                        RunOnMainThread(() => {
                            ShortcutTestFactory factory = new(testedCreature, otherCreature) { LocationGroup = locationGroup };
                            TestRunner runner = new() { ReportVerbosity = reportVerbosity, AssertNoLoggedErrors = assertNoLoggedErrors };
                            runner.RunTest(factory.CreateAllTestsForPair());
                        });
                    }

                    if (otherCreature == CreatureTemplate.Type.Slugcat)
                        ImGui.EndDisabled();

                    if (ImGui.Button("Setup test"))
                    {
                        RunOnMainThread(() => {
                            ShortcutTestFactory factory = new(testedCreature, otherCreature) { LocationGroup = locationGroup };
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

                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Matrix test"))
                {
                    if (testedCreatures == null || otherCreatures == null)
                    {
                        ImGUIComponents.InitTestedTemplateNames();
                        testedCreatures = new bool[ImGUIComponents.TestedTemplateNames.Length];
                        otherCreatures = new bool[ImGUIComponents.TestedTemplateNamesWithSlugcat.Length];
                    }

                    if (ImGui.BeginChild("ListBoxes", new System.Numerics.Vector2(-float.Epsilon, ImGui.GetTextLineHeightWithSpacing() * 8), ImGuiChildFlags.ResizeY))
                    {
                        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X * 0.49f);
                        ImGUIComponents.CreatureTemplateMultiPicker("Tested creatures", ref testedCreatures);
                        ImGui.SameLine();
                        ImGUIComponents.CreatureTemplateMultiPicker("Other creatures", ref otherCreatures, withSlugcat: true);
                        ImGui.PopItemWidth();
                    }
                    ImGui.EndChild();

                    if (game.GamePaused
                        || game.FirstAnyPlayer.realizedCreature == null
                        || game.FirstRealizedPlayer.inShortcut
                        || testedCreatures.Count(x => x) == 0
                        || otherCreatures.Count(x => x) == 0)
                    {
                        ImGui.BeginDisabled();
                    }

                    if (ImGui.Button("Run tests"))
                    {
                        var testedTemplates = ImGUIComponents.GetSelectedTemplates(testedCreatures);
                        var otherTemplates = ImGUIComponents.GetSelectedTemplates(otherCreatures, withSlugcat: true);

                        runningTests = true;
                        currentTestIndex = 0;
                        testStopwatch = new Stopwatch();
                        testStopwatch.Start();
                        startTicks = testStopwatch.ElapsedTicks;
                        testRunningInfo = $"{currentTestIndex}/{testedTemplates.Length} tests completed\n0.00 s passed";

                        void RunNextTest()
                        {
                            TestRunner runner = new() { ReportVerbosity = reportVerbosity, AssertNoLoggedErrors = assertNoLoggedErrors };
                            runner.RunTest(ShortcutTestFactory.CreateAllForTested(testedTemplates[currentTestIndex], otherTemplates, locationGroup), clearTestLog: currentTestIndex == 0);

                            currentTestIndex++;

                            TimeSpan timeTaken = TimeSpan.FromTicks(testStopwatch.ElapsedTicks - startTicks);
                            TimeSpan timeLeft = timeTaken.DivideBy(currentTestIndex).MultiplyBy(testedTemplates.Length - currentTestIndex);
                            testRunningInfo = $"{currentTestIndex}/{testedTemplates.Length} tests completed\n{timeTaken.TotalSeconds:0.00} s passed, {timeLeft.TotalSeconds:0.00} s remaining";

                            if (!Input.GetKey(KeyCode.I) && runningTests && currentTestIndex < testedTemplates.Length)
                            {
                                UtilityCore.Scheduler.Schedule(RunNextTest, frameInterval: 1, invokeLimit: 1);
                            }
                            else
                            {
                                runningTests = false;
                                TimeSpan totalTimeTaken = TimeSpan.FromTicks(testStopwatch.ElapsedTicks - startTicks);
                                TestRunner.CombinedLogger.LogDebug($"{currentTestIndex} tests completed in {(int)totalTimeTaken.TotalMilliseconds} ms / {totalTimeTaken.TotalSeconds:0.00} s");
                                testStopwatch.Stop();
                            }
                        }

                        UtilityCore.Scheduler.Schedule(RunNextTest, frameInterval: 1, invokeLimit: 1);
                    }

                    if (game.GamePaused
                        || game.FirstAnyPlayer.realizedCreature == null
                        || game.FirstRealizedPlayer.inShortcut
                        || testedCreatures.Count(x => x) == 0
                        || otherCreatures.Count(x => x) == 0)
                    {
                        ImGui.EndDisabled();
                    }

                    if (runningTests)
                    {
                        ImGui.SameLine();
                        ImGui.TextDisabled("Hold I to stop");
                    }

                    if (!string.IsNullOrEmpty(testRunningInfo))
                    {
                        ImGui.Text(testRunningInfo);
                    }

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
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
