using ImGuiNET;
using LogUtils.Formatting;
using MorePipeJukeNerfs.Debug.Tests;
using MorePipeJukeNerfs.Shortcuts;
using RWCustom;
using System.Diagnostics.CodeAnalysis;

namespace MorePipeJukeNerfs.Debug;

internal static class ImGUIComponents
{
    public static void EnumPicker<T>(string label, ref T value) where T: Enum
    {
        string[] names = Enum.GetNames(typeof(T));

        int index = Convert.ToInt32(value);
        ImGui.Combo(label, ref index, names, names.Length);
        value = (T)Enum.ToObject(typeof(T), index);
    }

    public static void CreatureInfo(AbstractCreature cr)
    {
        if (ImGui.TreeNode($"{cr} in {cr.Room.name}###{cr.ID.number}"))
        {
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
                SuckIntoShortcut(real);
                ImGui.Indent();
                ImGui.TreePop();
            }
            ImGui.TreePop();
        }
    }

    private static int[] shortcutPos = new int[2]; // TODO: Separate inputs
    public static void SuckIntoShortcut(Creature real)
    {
        ImGui.SetNextItemWidth(-165);
        ImGui.InputInt2("", ref shortcutPos[0]);
        ImGui.SameLine();
        if (ImGui.Button("Suck into shortcut"))
        {
            real.SuckedIntoShortCut(new IntVector2(shortcutPos[0], shortcutPos[1]), false);
        }
    }

    public static void CreatureTemplatePicker(string label, ref CreatureTemplate.Type type, bool other = false)
    {
        TestedCreatures.InitTestedTemplateNames();
        string[] usedNames = other ? TestedCreatures.OtherTemplateNames: TestedCreatures.TestedTemplateNames;
        int index = usedNames.IndexOf(type.value);
        ImGui.Combo(label, ref index, usedNames, usedNames.Length);
        type = new CreatureTemplate.Type(usedNames[index]);
    }

    public static void CreatureTemplateMultiPicker(string label, ref bool[] selectedTypes, bool other = false)
    {
        TestedCreatures.InitTestedTemplateNames();
        string[] usedNames = other ? TestedCreatures.OtherTemplateNames : TestedCreatures.TestedTemplateNames;

        ImGui.BeginGroup();
        ImGui.Text($"{label}: {selectedTypes.Count(x => x)}/{usedNames.Length}");
        ImGui.SameLine();
        ImGui.TextDisabled("(?)");
        ImGui.SetItemTooltip("Use Ctrl/Shift to select multiple, Ctrl+A to select all");
        if (ImGui.BeginListBox($"##{label}", new System.Numerics.Vector2(0f, -5f)))
        {
            ImGuiMultiSelectIOPtr msIo = ImGui.BeginMultiSelect(
                ImGuiMultiSelectFlags.ClearOnEscape | ImGuiMultiSelectFlags.BoxSelect1d,
                selectedTypes.Count(x => x),
                usedNames.Length
            );
            // Applying requests
            for (int i = 0; i < msIo.Requests.Size; i++)
            {
                ImGuiSelectionRequestPtr req = msIo.Requests[i];

                if (req.Type == ImGuiSelectionRequestType.SetAll)
                {
                    for (int j = 0; j < selectedTypes.Length; j++)
                    {
                        selectedTypes[j] = req.Selected;
                    }
                }
                else if (req.Type == ImGuiSelectionRequestType.SetRange)
                {
                    for (int j = (int)req.RangeFirstItem; j <= (int)req.RangeLastItem; j++)
                    {
                        selectedTypes[j] = req.Selected;
                    }
                }
            }

            for (int i = 0; i < usedNames.Length; i++)
            {
                ImGui.SetNextItemSelectionUserData(i);
                ImGui.Selectable(usedNames[i], selectedTypes[i]);
            }

            msIo = ImGui.EndMultiSelect();
            // Applying requests
            for (int i = 0; i < msIo.Requests.Size; i++)
            {
                ImGuiSelectionRequestPtr req = msIo.Requests[i];

                if (req.Type == ImGuiSelectionRequestType.SetAll)
                {
                    for (int j = 0; j < selectedTypes.Length; j++)
                    {
                        selectedTypes[j] = req.Selected;
                    }
                }
                else if (req.Type == ImGuiSelectionRequestType.SetRange)
                {
                    for (int j = (int)req.RangeFirstItem; j <= (int)req.RangeLastItem; j++)
                    {
                        selectedTypes[j] = req.Selected;
                    }
                }
            }

            ImGui.EndListBox();
        }
        ImGui.EndGroup();
    }
}
