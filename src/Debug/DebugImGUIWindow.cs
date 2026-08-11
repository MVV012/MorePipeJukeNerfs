using ImGuiNET;
using LogUtils;
using LogUtils.Enums;
using LogUtils.Helpers;
using MonoMod.RuntimeDetour;
using MorePipeJukeNerfs.Debug.Tests;
using MorePipeJukeNerfs.Shortcuts;
using RWCustom;
using RWIMGUI.API;

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

            ImGui.InputInt("Update count", ref updates);
            if (ImGui.Button($"Update game {updates} times###update_button"))
            {
                RunOnMainThread(() => game.Update(updates));
            }

            if (ImGui.Button("Clear log"))
            {
                LogFile.StartNewSession(LogUtilsLogger.ID);
            }

            ImGui.Indent();
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeEx("Testing", ImGuiTreeNodeFlags.Framed))
        {
            ImGui.Unindent();

            ImGUIComponents.CreatureTemplatePicker("Tested creature", ref testedCreature);
            ImGUIComponents.CreatureTemplatePicker("Other creature", ref otherCreature);

            ImGui.Separator();

            ImGUIComponents.EnumPicker("Report verbosity", ref reportVerbosity);
            ImGui.Checkbox("Assert no logged errors", ref assertNoLoggedErrors);

            if (game.GamePaused)
            {
                ImGui.BeginDisabled();
            }

            if (ImGui.Button("Run all tests"))
            {
                RunOnMainThread(() => {
                    ShortcutTestFactory factory = new()
                    {
                        TestedType = testedCreature,
                        OtherType = otherCreature,
                    };
                    ShortcutTestBase.AssertNoLoggedErrors = assertNoLoggedErrors;
                    TestRunner runner = new() { ReportVerbosity = reportVerbosity };
                    runner.RunTests(factory.CreateAll());
                });
            }
            ImGui.SameLine();
            if (ImGui.Button("Setup first test"))
            {
                RunOnMainThread(() => {
                    ShortcutTestFactory factory = new()
                    {
                        TestedType = testedCreature,
                        OtherType = otherCreature,
                    };
                    factory.CreateAll().AllCases.OfType<ShortcutTestBase>().First().Setup();
                });
            }

            if (game.GamePaused)
            {
                ImGui.EndDisabled();
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
