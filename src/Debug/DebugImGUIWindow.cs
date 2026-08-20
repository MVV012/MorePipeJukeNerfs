using ImGuiNET;
using LogUtils;
using LogUtils.Diagnostics;
using LogUtils.Enums;
using LogUtils.Helpers;
using LogUtils.Policy;
using MonoMod.RuntimeDetour;
using MorePipeJukeNerfs.Debug.Tests;
using MorePipeJukeNerfs.Shortcuts;
using RWCustom;
using RWIMGUI.API;
using System.Diagnostics;
using UnityEngine;
using Vector4 = System.Numerics.Vector4;

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
        if (RunningTests) return false;

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
    private static ShortcutTestLocationGroup locationGroup = ShortcutTestLocationGroup.MPJNTST;
    private static bool[]? testedCreatures = null;
    private static bool[]? otherCreatures = null;

    public static bool RunningTests { get; private set; } = false;
    private static int currentTestIndex = -1;
    private static long startTicks = -1;
    private static Stopwatch testStopwatch = null!;
    private static string? testRunningInfo = null;
    private static bool showResults = false;
    private static TestableGroup allTestsGroup = null!;
    private static List<TestableGroup> allTests = [];
    private static List<TestableGroup> completedTests = [];
    private static TestRunner testRunner = null!;

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
                    ImGUIComponents.CreatureTemplatePicker("Other creature", ref otherCreature, other: true);
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
                        TestedCreatures.InitTestedTemplateNames();
                        testedCreatures = new bool[TestedCreatures.TestedTemplateNames.Length];
                        otherCreatures = new bool[TestedCreatures.OtherTemplateNames.Length];
                    }

                    if (RunningTests)
                        ImGui.BeginDisabled();

                    if (ImGui.BeginChild("ListBoxes", new System.Numerics.Vector2(-float.Epsilon, ImGui.GetTextLineHeightWithSpacing() * 8), ImGuiChildFlags.ResizeY))
                    {
                        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X * 0.49f);
                        ImGUIComponents.CreatureTemplateMultiPicker("Tested creatures", ref testedCreatures);
                        ImGui.SameLine();
                        ImGUIComponents.CreatureTemplateMultiPicker("Other creatures", ref otherCreatures, other: true);
                        ImGui.PopItemWidth();
                    }
                    ImGui.EndChild();

                    if (RunningTests)
                        ImGui.EndDisabled();

                    ImGui.BeginDisabled(game.GamePaused
                        || RunningTests
                        || game.FirstAnyPlayer.realizedCreature == null
                        || game.FirstRealizedPlayer.inShortcut
                        || testedCreatures.Count(x => x) == 0
                        || otherCreatures.Count(x => x) == 0);

                    if (ImGui.Button("Run tests"))
                    {
                        var testedTemplates = TestedCreatures.GetSelectedTemplates(testedCreatures);
                        var otherTemplates = TestedCreatures.GetSelectedTemplates(otherCreatures, other: true);

                        allTestsGroup = new TestableGroup($"{testedTemplates.Length} creatures <-> {otherTemplates.Length} creatures");

                        allTests = testedTemplates
                            .Select(tested => ShortcutTestFactory.CreateAllForTested(tested, otherTemplates, locationGroup, allTestsGroup))
                            .SelectMany(group => group.TestableCases)
                            .ToList();

                        testRunner = new TestRunner { ReportVerbosity = reportVerbosity, AssertNoLoggedErrors = assertNoLoggedErrors };

                        RunningTests = true;
                        currentTestIndex = 0;
                        completedTests = [];
                        testStopwatch = new Stopwatch();
                        testStopwatch.Start();
                        startTicks = testStopwatch.ElapsedTicks;
                        testRunningInfo = $"{currentTestIndex}/{allTests.Count} tests completed\n0.00 s passed";

                        void RunNextTest()
                        {
                            if (currentTestIndex == 0)
                            {
                                LogUtils.Policy.DebugPolicy.ShowDebugLog = false;
                                LogFile.StartNewSession(LogUtilsLogger.ID);
                                LogFile.StartNewSession(TestRunner.TestLogID);
                                TestRunner.CombinedLogger.LogDebug($"Starting matrix test:");
                                TestRunner.CombinedLogger.LogDebug($"{testedTemplates.Length} tested: {string.Join(", ", testedTemplates.Select(t => t.value))}");
                                TestRunner.CombinedLogger.LogDebug($"{otherTemplates.Length} other: {string.Join(", ", otherTemplates.Select(t => t.value))}");
                            }

                            var curTest = allTests[currentTestIndex];
                            curTest.DontLogReport = true;
                            testRunner.RunTest(curTest, clearTestLog: false);
                            completedTests.Add(curTest);
                            currentTestIndex++;

                            TimeSpan timeTaken = TimeSpan.FromTicks(testStopwatch.ElapsedTicks - startTicks);
                            TimeSpan timeLeft = timeTaken.DivideBy(currentTestIndex).MultiplyBy(allTests.Count - currentTestIndex);
                            testRunningInfo = $"{currentTestIndex}/{allTests.Count} tests completed\n{timeTaken.TotalSeconds:0.00} s passed, {timeLeft.TotalSeconds:0.00} s remaining";

                            if (!Input.GetKey(KeyCode.I) && RunningTests && currentTestIndex < allTests.Count)
                            {
                                UtilityCore.Scheduler.Schedule(RunNextTest, frameInterval: 1, invokeLimit: 1);
                            }
                            else
                            {
                                RunningTests = false;
                                TestCasePolicy.ReportVerbosity = reportVerbosity;
                                TimeSpan totalTimeTaken = TimeSpan.FromTicks(testStopwatch.ElapsedTicks - startTicks);
                                testStopwatch.Stop();

                                TestRunner.TestLogger.LogDebug("--- RESULTS ---");
                                TestRunner.TestLogger.LogDebug(allTestsGroup.CreateReport());

                                TestRunner.CombinedLogger.LogDebug($"{currentTestIndex} tests completed in {(int)totalTimeTaken.TotalMilliseconds} ms / {totalTimeTaken.TotalSeconds:0.00} s");
                                showResults = true;
                                LogUtils.Policy.DebugPolicy.ShowDebugLog = true;
                            }
                        }

                        UtilityCore.Scheduler.Schedule(RunNextTest, frameInterval: 1, invokeLimit: 1);
                    }

                    ImGui.EndDisabled();

                    if (RunningTests)
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

            if (showResults)
            {
                showResults = false;
                ImGui.OpenPopup("Matrix test results");
            }

            if (ImGui.BeginPopupModal("Matrix test results"))
            {
                var testedTemplates = TestedCreatures.GetSelectedTemplates(testedCreatures!);
                var otherTemplates = TestedCreatures.GetSelectedTemplates(otherCreatures!, other: true);

                if (ImGui.BeginTable("resultsTable", otherTemplates.Length + 1,
                    ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY
                    | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.BordersInnerH |  ImGuiTableFlags.HighlightHoveredColumn,
                    new System.Numerics.Vector2(0f, -30f)))
                {
                    ImGui.TableSetupColumn("");
                    foreach (var template in otherTemplates)
                    {
                        ImGui.TableSetupColumn(template.value, ImGuiTableColumnFlags.AngledHeader | ImGuiTableColumnFlags.WidthFixed);
                    }

                    ImGui.TableAngledHeadersRow();
                    for (int row = 0; row < testedTemplates.Length; row++)
                    {
                        ImGui.PushID(row);
                        ImGui.TableNextRow();

                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(testedTemplates[row].value);

                        for (int column = 0; column < otherTemplates.Length; column++)
                        {
                            if (row * otherTemplates.Length + column < completedTests.Count)
                            {
                                ImGui.PushID(column);
                                ImGui.TableSetColumnIndex(column + 1);

                                ShortcutTestBase[] tests = completedTests[row * otherTemplates.Length + column].AllCases.OfType<ShortcutTestBase>().GroupBy(test => test.Name).Select(g => g.First()).ToArray();

                                ShortcutTestResult res = tests.Select(test => test.GetResult()).Max();

                                Vector4 bgColor = res switch
                                {
                                    ShortcutTestResult.Passed => new(0, 1, 0, 1), // Green
                                    ShortcutTestResult.PassedWithException => new(1, 1, 0, 1), // Yellow
                                    ShortcutTestResult.Failed => new(1, 0, 0, 1), // Red
                                    ShortcutTestResult.SetupFailed => new(0.5f, 0, 1, 1), // Purple
                                    ShortcutTestResult.UnhandledException => new(0, 0, 0, 1), // Black
                                    _ => new(0.5f, 0.5f, 0.5f, 1)
                                };
                                Vector4 textColor = res switch
                                {
                                    ShortcutTestResult.PassedWithException => new(0, 0, 0, 1),
                                    _ => new(1, 1, 1, 1)
                                };

                                ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, ImGui.GetColorU32(bgColor));
                                ImGui.TextColored(textColor, $"{tests.Count(test => !test.HasFailed())}");

                                ImGui.PopID();
                            }
                        }

                        ImGui.PopID();
                    }

                    ImGui.EndTable();
                }

                if (ImGui.Button("Close"))
                    ImGui.CloseCurrentPopup();

                ImGui.EndPopup();
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
