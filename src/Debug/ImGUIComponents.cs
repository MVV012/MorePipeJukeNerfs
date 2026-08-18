using ImGuiNET;
using LogUtils.Formatting;
using MorePipeJukeNerfs.Shortcuts;
using RWCustom;

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

    private static string[]? testedTemplateNames;
    private static string[]? testedTemplateNamesWithSlugcat;
    public static void CreatureTemplatePicker(string label, ref CreatureTemplate.Type type, bool withSlugcat = false)
    {
        if (testedTemplateNames == null || testedTemplateNamesWithSlugcat == null)
        {
            string[] excludedCreatures = ["LizardTemplate", "TentaclePlant", "Overseer", "MirosVulture", "Inspector", "DrillCrab", "TowerCrab", "BoxWorm", "Rattler", "GrappleSnake", "RippleSpider"];

            testedTemplateNames = StaticWorld.creatureTemplates
                .Where(ct => ct.AI && !ct.smallCreature && !ct.abstractImmobile && !ct.forbidStandardShortcutEntry)
                .Select(ct => ct.type.value)
                .Where(name => !excludedCreatures.Contains(name))
                .ToArray();
            testedTemplateNamesWithSlugcat = ["Slugcat", ..testedTemplateNames];
            Log.LogDebug($"{testedTemplateNames.Length} tested creatures: {string.Join(", ", testedTemplateNames)}");
        }
        string[] usedNames = withSlugcat ? testedTemplateNamesWithSlugcat : testedTemplateNames;

        int index = usedNames.IndexOf(type.value);
        ImGui.Combo(label, ref index, usedNames, usedNames.Length);
        type = new CreatureTemplate.Type(usedNames[index]);
    }
}
