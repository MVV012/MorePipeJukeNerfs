using System.Diagnostics.CodeAnalysis;

namespace MorePipeJukeNerfs.Debug.Tests;

internal static class TestedCreatures
{
    public static string[]? TestedTemplateNames;
    public static string[]? OtherTemplateNames;

    [MemberNotNull(nameof(TestedTemplateNames))]
    [MemberNotNull(nameof(OtherTemplateNames))]
    public static void InitTestedTemplateNames()
    {
        if (TestedTemplateNames != null && OtherTemplateNames != null)
        {
            return;
        }

        string[] excludedCreatures = ["StandardGroundCreature", "LizardTemplate", "PoleMimic", "TentaclePlant", "Overseer", "MirosVulture", "Inspector", "DrillCrab", "TowerCrab", "BoxWorm", "Rattler", "GrappleSnake", "RippleSpider"];
        string[] excludedFromTested = ["TubeWorm", "Barnacle", "Rat", "Frog", "Snail"];

        TestedTemplateNames = StaticWorld.creatureTemplates
            .Where(ct => ct.AI && !ct.smallCreature && !ct.abstractImmobile && !ct.forbidStandardShortcutEntry)
            .Select(ct => ct.type.value)
            .Where(name => !excludedCreatures.Contains(name) && !excludedFromTested.Contains(name))
            .ToArray();

        OtherTemplateNames = StaticWorld.creatureTemplates
            .Where(ct => !ct.smallCreature && !ct.abstractImmobile && !ct.forbidStandardShortcutEntry)
            .Select(ct => ct.type.value)
            .Where(name => !excludedCreatures.Contains(name))
            .ToArray();

        Log.LogDebug($"{TestedTemplateNames.Length} tested creatures: {string.Join(", ", TestedTemplateNames)}");
        Log.LogDebug($"{OtherTemplateNames.Length} other creatures: {string.Join(", ", OtherTemplateNames)}");
    }

    public static CreatureTemplate.Type[] GetSelectedTemplates(bool[] selectedTypes, bool other = false)
    {
        InitTestedTemplateNames();
        string[] usedNames = other ? OtherTemplateNames : TestedTemplateNames;
        return usedNames
            .Where((name, i) => selectedTypes[i])
            .Select(name => new CreatureTemplate.Type(name))
            .ToArray();
    }
}
