using System.Runtime.CompilerServices;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using MorePipeJukeNerfs.Remix;

namespace MorePipeJukeNerfs;

public class Options : OptionInterface
{
    public static Options Instance { get; } = new Options();

    public static Configurable<bool> ShortcutNoticeCreatures { get; } = Bind(true, "When two creatures enter shortcut from opposite ends and meet, they are made aware of each other");
    public static Configurable<bool> ShortcutNoticeUnseen { get; } = Bind(true, "Creature that didn't know about another creature before meeting will notice it");
    public static Configurable<bool> ShortcutNoticeSeen { get; } = Bind(true, "Creature that already knew about another creature before meeting will be made aware of it's position and state");
    public static Configurable<bool> ShortcutNoticeOnlyPlayer { get; } = Bind(false, "Only player will be noticed when creatures meet in shortcut");

    public static Configurable<bool> PredictableShortcuts { get; } = Bind(true, "When creature sees other entering shortcut, it will know exactly that other will come out from other side");
    public static Configurable<bool> PredictableShortcutsOnlyPlayer { get; } = Bind(false, "More predictable shortcuts option will only work for creatures seeing player");

    public static Configurable<bool> IncreaseShortcutDelay { get; } = Bind(true, "Increases delay before player can enter shortcut again dependent on how many times same shortcut was used");
    public static Configurable<int> ShortcutDelayStarting { get; } = Bind(20, min: 20, max: 999, "Starting shortcut delay, by default equal to unmodded value");
    public static Configurable<int> ShortcutDelayIncrease { get; } = Bind(10, min: 0, max: 999, "Shortcut delay will increase by this amount for each shortcut use");
    public static Configurable<int> ShortcutDelayShortcutUses { get; } = Bind(5, min: 2, max: 99, "Shortcut delay will start increasing after using same shortcut this many times");
    public static Configurable<int> ShortcutDelayMax { get; } = Bind(60, min: 20, max: 999, "Maximum shortcut delay that can be reached");

    public static Configurable<bool> ReduceInvincibility { get; } = Bind(true, "Reduces player's invincibility after exiting shortcut dependent on how many times same shortcut was used");
    public static Configurable<int> InvincibilityStarting { get; } = Bind(40, min: 0, max: 40, "Starting invincibility duration, by default equal to unmodded value");
    public static Configurable<int> InvincibilityReduction { get; } = Bind(10, min: 0, max: 40, "Invincibility will decrease by this amount for each shortcut use");
    public static Configurable<int> InvincibilityShortcutUses { get; } = Bind(8, min: 2, max: 99, "Invincibility will start decreasing after using same shortcut this many times");
    public static Configurable<int> InvincibilityMin { get; } = Bind(0, min: 0, max: 40, "Minimal invincibility duration that can be reached");

    public static Configurable<int> ShortcutUsesResetTime { get; } = Bind(8 * 40, min: 1 * 40, max: 60 * 40, "Amount of repeating uses of same shortcut resets after not using it for this time"); // For now unconfigurable

    public override void Initialize()
    {
        base.Initialize();

        OpTab tab = new OpTab(this);
        Tabs = [tab];

        float offsetY = 10;
        List<UIelement> list = UIQueue.InitializeQueues(tab, 40, ref offsetY,
            new LabelQueue($"More Pipe Juke Nerfs", FLabelAlignment.Center, bigText: true) { Height = 30 },

            new CheckBoxQueue("Creatures notice other creatures they meet in shortcut", ShortcutNoticeCreatures) { OffsetX = -10 },
            new CheckBoxQueue("Creatures that were not seen before are noticed", ShortcutNoticeUnseen),
            new CheckBoxQueue("Creatures that were already seen are noticed", ShortcutNoticeSeen),
            new CheckBoxQueue("Only player is noticed", ShortcutNoticeOnlyPlayer),

            new SpaceQueue(22),

            new CheckBoxQueue("More predictable shortcuts", PredictableShortcuts) { OffsetX = -10 },
            new CheckBoxQueue("More predictable shortcuts only for player", PredictableShortcutsOnlyPlayer),

            new SpaceQueue(22),

            new CheckBoxQueue("Increase delay before player can enter shortcut after exiting one:", IncreaseShortcutDelay) { OffsetX = -10, RightFocusThis = false },
            new DurationInputQueue("Starting delay:", ShortcutDelayStarting) { LeftFocusTab = false, RightFocusThis = false },
            new DurationAndShortcutUsesQueue("Increase by", ShortcutDelayIncrease, ShortcutDelayShortcutUses) { LeftFocusTab = false },
            new DurationInputQueue("Maximum delay:", ShortcutDelayMax) { LeftFocusTab = false, RightFocusThis = false },

            new SpaceQueue(22),

            new CheckBoxQueue("Reduce invincibility player gets after exiting shortcut:", ReduceInvincibility) { OffsetX = -10, RightFocusThis = false },
            new DurationInputQueue("Starting invincibility:", InvincibilityStarting) { LeftFocusTab = false, RightFocusThis = false },
            new DurationAndShortcutUsesQueue("Reduce by", InvincibilityReduction, InvincibilityShortcutUses) { LeftFocusTab = false },
            new DurationInputQueue("Minimal invincibility:", InvincibilityMin) { LeftFocusTab = false, RightFocusThis = false }
        );

        BindDependentConfigs(list, ShortcutNoticeCreatures, [
            ShortcutNoticeUnseen,
            ShortcutNoticeSeen,
            ShortcutNoticeOnlyPlayer
        ], updateFocusables: true);
        BindDependentConfigs(list, PredictableShortcuts, [
            PredictableShortcutsOnlyPlayer
        ], updateFocusables: true);
        BindDependentConfigs(list, ReduceInvincibility, [
            InvincibilityStarting,
            InvincibilityReduction,
            InvincibilityShortcutUses,
            InvincibilityMin
        ], updateFocusables: true);
        BindDependentConfigs(list, IncreaseShortcutDelay, [
            ShortcutDelayStarting,
            ShortcutDelayIncrease,
            ShortcutDelayShortcutUses,
            ShortcutDelayMax
        ], updateFocusables: true);

        SetShortcutUsesFocusables(list.WithConfig(InvincibilityShortcutUses), list.WithConfig(InvincibilityReduction));
        SetShortcutUsesFocusables(list.WithConfig(ShortcutDelayShortcutUses), list.WithConfig(ShortcutDelayIncrease));

        list.OfType<UIfocusable>().First().SetNextFocusable(UIfocusable.NextDirection.Up,
             FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.RevertButton)
        );
        list.OfType<UIfocusable>().Last().SetNextFocusable(UIfocusable.NextDirection.Down,
             FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.RevertButton)
        );
    }

    private static Configurable<T> Bind<T>(T defaultValue, string? description = null, [CallerMemberName] string? key = null)
    {
        return Instance.config.Bind(key, defaultValue, new ConfigurableInfo(GetDescription(description, defaultValue)));
    }

    private static Configurable<T> Bind<T>(T defaultValue, T min, T max, string? description = null, [CallerMemberName] string? key = null) where T : IComparable
    {
        return Instance.config.Bind(key, defaultValue, new ConfigurableInfo(GetDescription(description, defaultValue), new ConfigAcceptableRange<T>(min, max)));
    }

    private static string GetDescription<T>(string? description, T defaultValue)
    {
        string defaultString;
        if (defaultValue is bool defaultBool)
        {
            defaultString = defaultBool ? "Enabled" : "Disabled";
        }
        else
        {
            defaultString = defaultValue!.ToString();
        }
        return string.IsNullOrWhiteSpace(description) ? $"Default: {defaultString}" : $"{description}  -  Default: {defaultString}";
    }

    private static void BindDependentConfigs(List<UIelement> list, Configurable<bool> mainConfig, List<ConfigurableBase> otherConfigs, bool updateFocusables = false)
    {
        OpCheckBox mainElement = (OpCheckBox)list.WithConfig(mainConfig);
        List<UIconfig> dependentElements = otherConfigs.Select(config => list.WithConfig(config)).ToList();

        UpdateDependentElements(mainElement.GetValueBool());
        mainElement.OnValueUpdate += (_, _, _) => UpdateDependentElements(mainElement.GetValueBool());

        void UpdateDependentElements(bool active)
        {
            foreach (UIconfig element in dependentElements)
            {
                element.greyedOut = !active;
            }
            if (updateFocusables)
            {
                UIconfig first = dependentElements[0];
                UIconfig last = dependentElements[^1];

                UIfocusable? beforeFirst = first.NextFocusable[(int)UIfocusable.NextDirection.Up];
                UIfocusable? afterLast = last.NextFocusable[(int)UIfocusable.NextDirection.Down];

                if (beforeFirst != null && afterLast != null)
                {
                    if (!active)
                    {
                        UIfocusable.MutualVerticalFocusableBind(afterLast, beforeFirst);
                    }
                    else
                    {
                        UIfocusable.MutualVerticalFocusableBind(first, beforeFirst);
                        UIfocusable.MutualVerticalFocusableBind(afterLast, last);
                    }
                }
                else if (beforeFirst != null)
                {
                    if (!active)
                    {
                        beforeFirst.SetNextFocusable(UIfocusable.NextDirection.Down,
                            FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.RevertButton)
                        );
                    }
                    else
                    {
                        beforeFirst.SetNextFocusable(UIfocusable.NextDirection.Down, first);
                    }
                }
            }
        }
    }

    private void SetShortcutUsesFocusables(UIfocusable shortcutUsesElement, UIfocusable mainFocusable)
    {
        shortcutUsesElement.SetNextFocusable(UIfocusable.NextDirection.Up, mainFocusable.NextFocusable[(int)UIfocusable.NextDirection.Up]);
        shortcutUsesElement.SetNextFocusable(UIfocusable.NextDirection.Down, mainFocusable.NextFocusable[(int)UIfocusable.NextDirection.Down]);

        mainFocusable.NextFocusable[(int)UIfocusable.NextDirection.Up].SetNextFocusable(UIfocusable.NextDirection.Right, shortcutUsesElement);
        mainFocusable.NextFocusable[(int)UIfocusable.NextDirection.Down].SetNextFocusable(UIfocusable.NextDirection.Right, shortcutUsesElement);
    }

    private Options(): base() {}
}
