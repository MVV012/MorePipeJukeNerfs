using ImGuiNET;
using LogUtils;
using MonoMod.RuntimeDetour;
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
    private static void WindowContent()
    {
        if (!RWGameUtils.TryGetRWGame(out RainWorldGame game))
        {
            ImGui.Text("Game is not opened!");
            return;
        }

        if (ImGui.Button("Murder everyone"))
        {
            MouseDrag.Destroy.DestroyRegionObjects(game, creatures: true, items: true);
        }

        ImGui.InputInt("Update count", ref updates);
        if (ImGui.Button($"Update game {updates} times###update_button"))
        {
            UtilityCore.Scheduler.Schedule(() =>
            {
                for (int i = 0; i < updates; i++)
                {
                    game.Update();
                }
            },
            frameInterval: 1, invokeLimit: 1);
        }

        if (ImGui.TreeNodeEx("Creatures", ImGuiTreeNodeFlags.Framed))
        {
            ImGui.Unindent();

            foreach (var cr in game.world.abstractRooms
                .SelectMany(room => room.creatures)
                .Where(cr => !cr.creatureTemplate.smallCreature)
                .OrderBy(cr => cr.ID.number))
            {
                CreatureInfoComponent(cr);
            }

            ImGui.Indent();
            ImGui.TreePop();
        }
    }

    private static void CreatureInfoComponent(AbstractCreature cr)
    {
        if (ImGui.TreeNode($"{cr} in {cr.Room.name}###{cr.ID.number}")) {
            ImGui.Text(cr.pos.ToString());

            if (cr.realizedCreature == null)
            {
                ImGui.Text("Creature is not realized");
                ImGui.TreePop();
                return;
            }

            Creature real = cr.realizedCreature;
            ImGui.Text($"Main body chunk pos: {real.mainBodyChunk.pos}");

            string shortcutString =
                real.inShortcut && real.inShortcutVessel.TryGetShortcut(out IShortcut shortcut)
                ? shortcut.ToString()
                : "Not in shortcut";

            if (ImGui.TreeNode($"{shortcutString}###{cr.ID.number}"))
            {
                ImGui.Unindent();
                SuckIntoShortcutComponent(real);
                ImGui.Indent();
                ImGui.TreePop();
            }
            ImGui.TreePop();
        }
    }

    private static int[] shortcutPos = new int[2];
    private static void SuckIntoShortcutComponent(Creature real)
    {
        ImGui.SetNextItemWidth(-165);
        ImGui.InputInt2("", ref shortcutPos[0]);
        ImGui.SameLine();
        if (ImGui.Button("Suck into shortcut"))
        {
            real.SuckedIntoShortCut(new IntVector2(shortcutPos[0], shortcutPos[1]), false);
        }
    }
}
