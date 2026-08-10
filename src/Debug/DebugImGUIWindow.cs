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
    private static CreatureTemplate.Type testedCreature = CreatureTemplate.Type.PinkLizard;
    private static CreatureTemplate.Type otherCreature = CreatureTemplate.Type.BlueLizard;
    private static FormatEnums.FormatVerbosity reportVerbosity = FormatEnums.FormatVerbosity.Verbose;
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
                MouseDrag.Destroy.DestroyRegionObjects(game, creatures: true, items: true);
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

            if (!game.GamePaused)
            {
                if (ImGui.Button("Run test"))
                {
                    RunOnMainThread(() => {
                        TestRunner runner = new() { ReportVerbosity = reportVerbosity };
                        ShortcutTestFactory factory = new()
                        {
                            TestedType = testedCreature,
                            OtherType = otherCreature,
                        };
                        runner.RunTests(factory.CreateNormalGroup());
                    });
                }
            }
            else
            {
                ImGui.BeginDisabled();
                ImGui.Button("Unpause game");
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
