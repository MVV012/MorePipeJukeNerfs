
using Menu.Remix.MixedUI;
using UnityEngine;

namespace MorePipeJukeNerfs.Remix;

public class DurationAndShortcutUsesQueue : DurationInputQueue
{
    private Configurable<int> _shorcutUsesConfig;
    public float ShortcutUsesUpdownWidth { get; init; } = 56;

    public DurationAndShortcutUsesQueue(string text, Configurable<int> durationConfig, Configurable<int> shorcutUsesConfig) : base(text, durationConfig)
    {
        _shorcutUsesConfig = shorcutUsesConfig;
    }

    public override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
    {
        List<UIelement> list = base._InitializeThisQueue(holder, posX, ref offsetY);

        OpUpdown durationUpdown = (OpUpdown)list[0];
        OpLabel ticksSecondsLabel = (OpLabel)list[2];

        float posY = GetPosY(holder, offsetY);

        OpUpdown shortcutUsesUpdown = new OpUpdown(_shorcutUsesConfig, new Vector2(0, posY), ShortcutUsesUpdownWidth);
        if (!string.IsNullOrEmpty(_shorcutUsesConfig.info?.description))
        {
            shortcutUsesUpdown.description = GetFirstSentence(Translate(_shorcutUsesConfig.info!.description));
        }
        list.Add(shortcutUsesUpdown);
        UIfocusable.MutualHorizontalFocusableBind(durationUpdown, shortcutUsesUpdown);
        if (RightFocusThis)
        {
            shortcutUsesUpdown.SetNextFocusable(UIfocusable.NextDirection.Right, shortcutUsesUpdown);
        }
        shortcutUsesUpdown.ShowConfig();

        OpLabel startingFromLabel = new OpLabel(
            new Vector2(0, posY),
            new Vector2(120, 30),
            "starting from",
            FLabelAlignment.Left
        )
        {
            bumpBehav = shortcutUsesUpdown.bumpBehav,
            description = shortcutUsesUpdown.description
        };
        startingFromLabel.AdjustWidthToText();
        list.Add(startingFromLabel);

        OpLabel shortcutUsesLabel = new OpLabel(
            new Vector2(0, posY),
            new Vector2(200, 30),
            "uses of same shortcut",
            FLabelAlignment.Left
        )
        {
            bumpBehav = shortcutUsesUpdown.bumpBehav,
            description = shortcutUsesUpdown.description
        };
        shortcutUsesLabel.AdjustWidthToText();
        list.Add(shortcutUsesLabel);

        BindShortcutUsesLabel(shortcutUsesUpdown, shortcutUsesLabel);


        void AdjustElementPositions()
        {
            startingFromLabel.InstantSetX(ticksSecondsLabel.EndX + ElementSpacing);
            shortcutUsesUpdown.InstantSetX(startingFromLabel.EndX + ElementSpacing);
            shortcutUsesLabel.InstantSetX(shortcutUsesUpdown.EndX + ElementSpacing);
        }

        AdjustElementPositions();
        durationUpdown.OnValueUpdate += (_, _, _) => AdjustElementPositions();

        return list;
    }

    public static void BindShortcutUsesLabel(OpTextBox input, OpLabel label)
    {
        UpdateLabel();
        input.OnValueUpdate += (_, _, _) => UpdateLabel();

        void UpdateLabel()
        {
            label.text = $"use{(input.valueInt != 1 ? "s" : "")} of same shortcut";
            label.AdjustWidthToText();
        }
    }
}
